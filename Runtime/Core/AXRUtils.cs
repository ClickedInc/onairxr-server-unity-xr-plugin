using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AXRUtils {
    public static Dictionary<string, string> ParseCommandLine(string[] args) {
        if (args == null) { return null; }

        var result = new Dictionary<string, string>();
        foreach (var arg in args) {
            var split = arg.IndexOf("=");
            if (split <= 0) { continue; }

            var name = arg.Substring(0, split);
            var value = arg.Substring(split + 1);
            if (string.IsNullOrEmpty(name)) { continue; }

            result.Add(name, string.IsNullOrEmpty(value) == false ? value : null);
        }
        return result;
    }

    public static int ParseInt(string value, int defaultValue, Func<int, bool> predicate, Action<string> failed = null) {
        int result;
        if (int.TryParse(value, out result) && predicate(result)) {
            return result;
        }

        if (failed != null) {
            failed(value);
        }
        return defaultValue;
    }

    public static float ParseFloat(string value, float defaultValue, Func<float, bool> predicate, Action<string> failed = null) {
        float result;
        if (float.TryParse(value, out result) && predicate(result)) {
            return result;
        }

        if (failed != null) {
            failed(value);
        }
        return defaultValue;
    }
}
