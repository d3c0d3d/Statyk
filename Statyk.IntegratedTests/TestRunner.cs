using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XR.Std;

namespace Statyk.IntegratedTests
{
    public static class TestRunner
    {
        /// <summary>
        /// Execute Test Runner
        /// </summary>
        /// <param name="test"></param>
        public static void TestRun(Action test)
        {
            var testName = test.GetMethodInfo().Name
                .Replace("<", string.Empty)
                .Replace(">b__0", string.Empty)
                .Replace(">b__1", string.Empty)
                .Replace(">b__2", string.Empty);

            try
            {
                test();

                Cli.PrintLnC($"PASSED TEST '{testName}'", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                Cli.PrintLnC($"FAILED TEST '{testName}'", ConsoleColor.Red);
                Cli.PrintErrorMessage(ex);
            }
        }

        /// <summary>
        /// Tests if objects is Equals
        /// </summary>
        /// <typeparam name="T">obj</typeparam>
        /// <param name="obj">obj</param>
        /// <param name="other">obj</param>
        public static void IsEqualTo<T>(this T obj, T other)
        {
            if (!obj.FormatResult().Equals(other.FormatResult()))
                throw new Exception($"{obj}\n...different than...\n{other}");
        }

        /// <summary>
        /// Tests if Lists is Equals
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="other"></param>
        public static void IsSequenceEqualTo<T>(this IEnumerable<T> obj, IEnumerable<T> other)
        {
            if (!obj.SequenceEqual(other))
                throw new Exception($"{obj}\n...different than...\n{other}");
        }

        /// <summary>
        /// Tests if is 'False'
        /// </summary>
        /// <param name="condition"></param>
        public static void IsFalse(this bool condition)
        {
            if (condition) throw new Exception($"Expected 'false', {nameof(condition)} = {condition}");
        }

        /// <summary>
        /// Tests if is 'True'
        /// </summary>
        /// <param name="condition"></param>
        public static void IsTrue(this bool condition)
        {
            if (!condition) throw new Exception($"Expected 'true', {nameof(condition)} = {condition}");
        }

        /// <summary>
        /// Tests if is 'Null'
        /// </summary>
        /// <param name="obj"></param>
        public static void IsNull(this object obj)
        {
            if (obj != null) throw new Exception($"Expected 'null', {nameof(obj)} = {obj}");
        }
        
        private static object FormatResult(this object obj)
        {
            if (obj is string s)
            {
                obj = s.Replace("\r", "").Replace("\n", "");
            }
            return obj;
        }
    }
}
