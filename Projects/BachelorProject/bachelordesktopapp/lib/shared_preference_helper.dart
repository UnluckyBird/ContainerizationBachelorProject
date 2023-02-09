import 'package:shared_preferences/shared_preferences.dart';

class SharedPreferencesHelper {
  static const String _apiUrl = "language";
  static const String _suppressNotification = "suppressNotification";

  static Future<String> getAPIURL() async {
    final SharedPreferences prefs = await SharedPreferences.getInstance();

    return prefs.getString(_apiUrl) ?? '';
  }

  static Future<bool> setAPIURL(String value) async {
    final SharedPreferences prefs = await SharedPreferences.getInstance();

    return prefs.setString(_apiUrl, value);
  }

  static Future<bool> getSuppressNotification() async {
    final SharedPreferences prefs = await SharedPreferences.getInstance();

    return prefs.getBool(_suppressNotification) ?? false;
  }

  static Future<bool> setSuppressNotification(bool value) async {
    final SharedPreferences prefs = await SharedPreferences.getInstance();

    return prefs.setBool(_suppressNotification, value);
  }
}