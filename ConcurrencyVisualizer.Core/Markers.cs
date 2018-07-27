namespace Microsoft.ConcurrencyVisualizer.Instrumentation
{
    public static class Markers
    {
        public static MarkerSeries CreateMarkerSeries(string seriesName)
        {
            return DefaultWriter.CreateMarkerSeries(seriesName);
        }

        public static Span EnterSpan(string text)
        {
            return DefaultWriter.DefaultSeries.EnterSpan(text);
        }

        public static Span EnterSpan(Importance level, string text)
        {
            return DefaultWriter.DefaultSeries.EnterSpan(level, text);
        }

        public static Span EnterSpan(int category, string text)
        {
            return DefaultWriter.DefaultSeries.EnterSpan(category, text);
        }

        public static Span EnterSpan(string format, params object[] args)
        {
            return DefaultWriter.DefaultSeries.EnterSpan(format, args);
        }

        public static Span EnterSpan(Importance level, int category, string text)
        {
            return DefaultWriter.DefaultSeries.EnterSpan(level, category, text);
        }

        public static Span EnterSpan(Importance level, string format, params object[] args)
        {
            return DefaultWriter.DefaultSeries.EnterSpan(level, format, args);
        }

        public static Span EnterSpan(int category, string format, params object[] args)
        {
            return DefaultWriter.DefaultSeries.EnterSpan(category, format, args);
        }

        public static Span EnterSpan(Importance level, int category, string format, params object[] args)
        {
            return DefaultWriter.DefaultSeries.EnterSpan(level, category, format, args);
        }

        public static bool IsEnabled()
        {
            return DefaultWriter.IsEnabled();
        }

        public static bool IsEnabled(Importance level)
        {
            return DefaultWriter.IsEnabled(level);
        }

        public static bool IsEnabled(int category)
        {
            return DefaultWriter.IsEnabled(category);
        }

        public static bool IsEnabled(Importance level, int category)
        {
            return DefaultWriter.IsEnabled(level, category);
        }

        public static void WriteAlert(string text)
        {
            DefaultWriter.DefaultSeries.WriteAlert(text);
        }

        public static void WriteAlert(string format, params object[] args)
        {
            DefaultWriter.DefaultSeries.WriteAlert(format, args);
        }

        public static void WriteFlag(string text)
        {
            DefaultWriter.DefaultSeries.WriteFlag(text);
        }

        public static void WriteFlag(Importance level, string text)
        {
            DefaultWriter.DefaultSeries.WriteFlag(level, text);
        }

        public static void WriteFlag(int category, string text)
        {
            DefaultWriter.DefaultSeries.WriteFlag(category, text);
        }

        public static void WriteFlag(string format, params object[] args)
        {
            DefaultWriter.DefaultSeries.WriteFlag(format, args);
        }

        public static void WriteFlag(Importance level, int category, string text)
        {
            DefaultWriter.DefaultSeries.WriteFlag(level, category, text);
        }

        public static void WriteFlag(Importance level, string format, params object[] args)
        {
            DefaultWriter.DefaultSeries.WriteFlag(level, format, args);
        }

        public static void WriteFlag(int category, string format, params object[] args)
        {
            DefaultWriter.DefaultSeries.WriteFlag(category, format, args);
        }

        public static void WriteFlag(Importance level, int category, string format, params object[] args)
        {
            DefaultWriter.DefaultSeries.WriteFlag(level, category, format, args);
        }

        public static void WriteMessage(string text)
        {
            DefaultWriter.DefaultSeries.WriteMessage(text);
        }

        public static void WriteMessage(Importance level, string text)
        {
            DefaultWriter.DefaultSeries.WriteMessage(level, text);
        }

        public static void WriteMessage(int category, string text)
        {
            DefaultWriter.DefaultSeries.WriteMessage(category, text);
        }

        public static void WriteMessage(string format, params object[] args)
        {
            DefaultWriter.DefaultSeries.WriteMessage(format, args);
        }

        public static void WriteMessage(Importance level, int category, string text)
        {
            DefaultWriter.DefaultSeries.WriteMessage(level, category, text);
        }

        public static void WriteMessage(Importance level, string format, params object[] args)
        {
            DefaultWriter.DefaultSeries.WriteMessage(level, format, args);
        }

        public static void WriteMessage(int category, string format, params object[] args)
        {
            DefaultWriter.DefaultSeries.WriteMessage(category, format, args);
        }

        public static void WriteMessage(Importance level, int category, string format, params object[] args)
        {
            DefaultWriter.DefaultSeries.WriteMessage(level, category, format, args);
        }

        public static MarkerWriter DefaultWriter { get; } = new MarkerWriter(MarkerWriter.DefaultProviderGuid);
    }
}

