#include <WiFi.h>
#include <HTTPClient.h>
#include <WiFiClientSecure.h>
#include <ArduinoJson.h>
#include <DHTesp.h>
#include <time.h>

const char* ssid = "Wokwi-GUEST";
const char* wifiPassword = "";

const char* apiBaseUrl = "https://sua-url-localtunnel.loca.lt";

const char* loginEmail = "gustavo@aruje.com";
const char* loginPassword = "Aruje123@";

// Sensores
const int DHT_PIN = 15;
const int SOIL_PIN = 34;
const int LDR_PIN = 35;

// Atuadores visuais
const int LED_OK_PIN = 25;
const int LED_ALERT_PIN = 26;
const int BUZZER_PIN = 27;

DHTesp dht;
WiFiClientSecure secureClient;

String jwtToken = "";
String sensorIds[10];
String sensorNames[10];
int sensorCount = 0;

unsigned long lastSend = 0;
const unsigned long sendInterval = 5000;

int sendCounter = 0;

void setup() {
  Serial.begin(115200);
  delay(1000);

  pinMode(LED_OK_PIN, OUTPUT);
  pinMode(LED_ALERT_PIN, OUTPUT);
  pinMode(BUZZER_PIN, OUTPUT);

  digitalWrite(LED_OK_PIN, LOW);
  digitalWrite(LED_ALERT_PIN, LOW);
  digitalWrite(BUZZER_PIN, LOW);

  dht.setup(DHT_PIN, DHTesp::DHT22);

  Serial.println("=================================");
  Serial.println("Aruje Wokwi IoT started");
  Serial.println("=================================");

  connectWiFi();

  secureClient.setInsecure();

  configTime(0, 0, "pool.ntp.org", "time.nist.gov");

  if (!login()) {
    Serial.println("Login failed. Check user/password/API.");
    alertError();
    return;
  }

  if (!loadSensors()) {
    Serial.println("No sensors found. Check seed/API.");
    alertError();
    return;
  }

  Serial.print("Sensors loaded: ");
  Serial.println(sensorCount);

  digitalWrite(LED_OK_PIN, HIGH);
}

void loop() {
  if (jwtToken == "" || sensorCount == 0) {
    delay(1000);
    return;
  }

  if (millis() - lastSend < sendInterval) {
    return;
  }

  lastSend = millis();

  int sensorIndex = sendCounter % sensorCount;

  sendReadingFromPhysicalSensors(
    sensorIds[sensorIndex],
    sensorNames[sensorIndex]
  );

  sendCounter++;
}

void connectWiFi() {
  Serial.print("Connecting to WiFi");

  WiFi.begin(ssid, wifiPassword);

  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }

  Serial.println();
  Serial.println("WiFi connected.");
  Serial.print("IP: ");
  Serial.println(WiFi.localIP());
}

bool login() {
  HTTPClient http;

  String url = String(apiBaseUrl) + "/api/auth/login";

  http.begin(secureClient, url);
  http.addHeader("Content-Type", "application/json");
  http.addHeader("Bypass-Tunnel-Reminder", "true");

  StaticJsonDocument<256> request;
  request["email"] = loginEmail;
  request["password"] = loginPassword;

  String body;
  serializeJson(request, body);

  Serial.println();
  Serial.println("Logging in...");

  int statusCode = http.POST(body);

  Serial.print("Login status: ");
  Serial.println(statusCode);

  String response = http.getString();

  if (statusCode != 200) {
    Serial.println("Login error response:");
    Serial.println(response);
    http.end();
    return false;
  }

  StaticJsonDocument<1024> json;
  DeserializationError error = deserializeJson(json, response);

  if (error) {
    Serial.print("Login JSON error: ");
    Serial.println(error.c_str());
    http.end();
    return false;
  }

  jwtToken = json["token"].as<String>();

  http.end();

  if (jwtToken == "") {
    Serial.println("Token not found in login response.");
    return false;
  }

  Serial.println("Login successful.");
  return true;
}

bool loadSensors() {
  HTTPClient http;

  String url = String(apiBaseUrl) + "/api/sensors";

  http.begin(secureClient, url);
  http.addHeader("Authorization", "Bearer " + jwtToken);
  http.addHeader("Bypass-Tunnel-Reminder", "true");

  Serial.println();
  Serial.println("Loading sensors...");

  int statusCode = http.GET();

  Serial.print("Sensors status: ");
  Serial.println(statusCode);

  String response = http.getString();

  if (statusCode != 200) {
    Serial.println("Sensors error response:");
    Serial.println(response);
    http.end();
    return false;
  }

  StaticJsonDocument<4096> json;
  DeserializationError error = deserializeJson(json, response);

  if (error) {
    Serial.print("Sensors JSON error: ");
    Serial.println(error.c_str());
    http.end();
    return false;
  }

  JsonArray sensors = json.as<JsonArray>();

  sensorCount = 0;

  for (JsonObject sensor : sensors) {
    if (sensorCount >= 10) {
      break;
    }

    sensorIds[sensorCount] = sensor["id"].as<String>();
    sensorNames[sensorCount] = sensor["name"].as<String>();

    Serial.print("Sensor found: ");
    Serial.print(sensorNames[sensorCount]);
    Serial.print(" | ");
    Serial.println(sensorIds[sensorCount]);

    sensorCount++;
  }

  http.end();

  return sensorCount > 0;
}

void sendReadingFromPhysicalSensors(String sensorId, String sensorName) {
  TempAndHumidity dhtData = dht.getTempAndHumidity();

  double temperature = dhtData.temperature;
  double airHumidity = dhtData.humidity;

  int soilRaw = analogRead(SOIL_PIN);
  int ldrRaw = analogRead(LDR_PIN);

  double soilMoisture = map(soilRaw, 0, 4095, 0, 100);
  double luminosity = map(ldrRaw, 0, 4095, 0, 1000);

  if (isnan(temperature) || isnan(airHumidity)) {
    Serial.println("Failed to read DHT22.");
    alertError();
    return;
  }

  bool critical = temperature >= 35 || soilMoisture <= 20;

  updateActuators(critical);

  sendSensorReading(
    sensorId,
    sensorName,
    temperature,
    airHumidity,
    soilMoisture,
    luminosity,
    critical
  );
}

void sendSensorReading(
  String sensorId,
  String sensorName,
  double temperature,
  double airHumidity,
  double soilMoisture,
  double luminosity,
  bool critical
) {
  HTTPClient http;

  String url = String(apiBaseUrl) + "/api/sensor-readings";

  http.begin(secureClient, url);
  http.addHeader("Content-Type", "application/json");
  http.addHeader("Authorization", "Bearer " + jwtToken);
  http.addHeader("Bypass-Tunnel-Reminder", "true");

  StaticJsonDocument<512> request;

  request["sensorId"] = sensorId;
  request["temperature"] = temperature;
  request["airHumidity"] = airHumidity;
  request["soilMoisture"] = soilMoisture;
  request["luminosity"] = luminosity;
  request["readingDate"] = getIsoDateTime();

  String body;
  serializeJson(request, body);

  Serial.println();
  Serial.println("=================================");
  Serial.print("Sending reading to: ");
  Serial.println(sensorName);

  Serial.print("Critical: ");
  Serial.println(critical ? "true" : "false");

  Serial.print("Temperature: ");
  Serial.println(temperature);

  Serial.print("Air humidity: ");
  Serial.println(airHumidity);

  Serial.print("Soil moisture: ");
  Serial.println(soilMoisture);

  Serial.print("Luminosity: ");
  Serial.println(luminosity);

  Serial.print("Payload: ");
  Serial.println(body);

  int statusCode = http.POST(body);

  Serial.print("Reading status: ");
  Serial.println(statusCode);

  String response = http.getString();

  if (statusCode >= 200 && statusCode < 300) {
    Serial.println("Reading sent successfully.");
  } else {
    Serial.println("Error sending reading:");
    Serial.println(response);

    if (statusCode == 401) {
      Serial.println("Token may be invalid. Trying login again...");
      login();
    }

    alertError();
  }

  http.end();
}

void updateActuators(bool critical) {
  if (critical) {
    digitalWrite(LED_OK_PIN, LOW);
    digitalWrite(LED_ALERT_PIN, HIGH);
    tone(BUZZER_PIN, 1000, 300);
  } else {
    digitalWrite(LED_OK_PIN, HIGH);
    digitalWrite(LED_ALERT_PIN, LOW);
    noTone(BUZZER_PIN);
  }
}

void alertError() {
  digitalWrite(LED_OK_PIN, LOW);

  for (int i = 0; i < 3; i++) {
    digitalWrite(LED_ALERT_PIN, HIGH);
    tone(BUZZER_PIN, 800, 150);
    delay(200);

    digitalWrite(LED_ALERT_PIN, LOW);
    noTone(BUZZER_PIN);
    delay(200);
  }
}

String getIsoDateTime() {
  struct tm timeinfo;

  if (!getLocalTime(&timeinfo)) {
    return "2026-06-22T15:30:00Z";
  }

  char buffer[25];

  strftime(buffer, sizeof(buffer), "%Y-%m-%dT%H:%M:%SZ", &timeinfo);

  return String(buffer);
}