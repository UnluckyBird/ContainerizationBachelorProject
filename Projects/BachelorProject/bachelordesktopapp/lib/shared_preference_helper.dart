import 'package:shared_preferences/shared_preferences.dart';

class SharedPreferencesHelper {
  static const String _apiUrl = "language";
  static const String _supressNotification = "supressNotification";

  static Future<String> getAPIURL() async {
    final SharedPreferences prefs = await SharedPreferences.getInstance();

    return prefs.getString(_apiUrl) ?? '';
  }

  static Future<bool> setAPIURL(String value) async {
    final SharedPreferences prefs = await SharedPreferences.getInstance();

    return prefs.setString(_apiUrl, value);
  }

  static Future<bool> getSupressNotification() async {
    final SharedPreferences prefs = await SharedPreferences.getInstance();

    return prefs.getBool(_supressNotification) ?? false;
  }

  static Future<bool> setSupressNotification(bool value) async {
    final SharedPreferences prefs = await SharedPreferences.getInstance();

    return prefs.setBool(_supressNotification, value);
  }
}