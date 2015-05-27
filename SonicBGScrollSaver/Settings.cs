using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using SonicRetro.SonLVL.API;
using System.Globalization;

namespace SonicBGScrollSaver
{
	public class Settings
	{
		[DefaultValue(true)]
		[IniAlwaysInclude]
		public bool PlayMusic { get; set; }
		[DefaultValue(100)]
		[IniAlwaysInclude]
		public int MusicVolume { get; set; }
		[DefaultValue(60)]
		[IniAlwaysInclude]
		public byte FramesPerSecond { get; set; }
		public bool FpsCounter { get; set; }
		[DefaultValue(4)]
		[IniAlwaysInclude]
		public short ScrollSpeed { get; set; }
		[TypeConverter(typeof(CustomTimeSpanConverter))]
		public TimeSpan DisplayTime { get; set; }
		public bool Shuffle { get; set; }
		[IniCollection(IniCollectionMode.NoSquareBrackets, StartIndex = 1)]
		[IniName("Level")]
		public List<string> Levels { get; set; }


		public static Settings Load()
		{
			if (File.Exists("SonicBGScrollSaver.ini"))
				return IniSerializer.Deserialize<Settings>("SonicBGScrollSaver.ini");
			else
			{
				Settings result = new Settings();
				result.PlayMusic = true;
				result.MusicVolume = 100;
				result.FramesPerSecond = 30;
				result.ScrollSpeed = 8;
				result.DisplayTime = TimeSpan.FromMinutes(5);
				return result;
			}
		}

		public void Save()
		{
			IniSerializer.Serialize(this, "SonicBGScrollSaver.ini");
		}
	}

	public class LevelInfo
	{
		[IniName("file")]
		public string FileName { get; set; }
		[IniName("type")]
		public string Type { get; set; }
		[IniName("name")]
		public string Name { get; set; }

		public static LevelInfo Load(string filename)
		{
			return IniSerializer.Deserialize<LevelInfo>(filename);
		}
	}

	public class CustomTimeSpanConverter : TypeConverter
	{
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(string))
				return true;
			return base.CanConvertTo(context, destinationType);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string) && value is TimeSpan)
			{
				TimeSpan a = (TimeSpan)value;
				List<string> result = new List<string>();
				bool negative = false;
				if (a < TimeSpan.Zero)
				{
					a = -a;
					negative = true;
				}
				if (a.Days > 0)
				{
					int days = a.Days % 365;
					int years = a.Days / 365;
					int weeks = days / 7;
					days %= 7;
					if (years > 0)
						result.Add(years + " year" + (years > 1 ? "s" : ""));
					if (weeks > 0)
						result.Add(weeks + " week" + (weeks > 1 ? "s" : ""));
					if (days > 0)
						result.Add(days + " day" + (days > 1 ? "s" : ""));
				}
				if (a.Hours > 0)
					result.Add(a.Hours + " hour" + (a.Hours > 1 ? "s" : ""));
				if (a.Minutes > 0)
					result.Add(a.Minutes + " minute" + (a.Minutes > 1 ? "s" : ""));
				if (a.Seconds > 0)
					result.Add(a.Seconds + " second" + (a.Seconds > 1 ? "s" : ""));
				if (a.Milliseconds > 0)
					result.Add(a.Milliseconds + " millisecond" + (a.Milliseconds > 1 ? "s" : ""));
				if (result.Count == 0)
					return "0 seconds";
				string resultstr = string.Join(", ", result.ToArray());
				if (negative)
					resultstr = "-" + resultstr;
				return resultstr;
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string))
				return true;
			return base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is string)
			{
				TimeSpan? ts = GetTimeSpan((string)value);
				if (!ts.HasValue)
					throw new FormatException("Value \"" + (string)value + "\" is not a valid TimeSpan.");
				return ts.Value;
			}
			return base.ConvertFrom(context, culture, value);
		}

		public override bool IsValid(ITypeDescriptorContext context, object value)
		{
			if (value is TimeSpan)
				return true;
			if (value is string)
				return GetTimeSpan((string)value).HasValue;
			return base.IsValid(context, value);
		}

		public static char[] numberchars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.' };

		static TimeSpan? GetTimeSpan(string str)
		{
			TimeSpan result = TimeSpan.Zero;
			if (TimeSpan.TryParse(str, out result))
				return result;
			str = str.Replace(",", "").Replace(" ", "");
			string num = string.Empty;
			string type = string.Empty;
			List<KeyValuePair<string, string>> parts = new List<KeyValuePair<string, string>>();
			bool negative = false;
			if (str.StartsWith("-"))
			{
				negative = true;
				str = str.Substring(1);
			}
			foreach (char item in str)
			{
				if (Array.IndexOf(numberchars, item) > -1)
				{
					if (!string.IsNullOrEmpty(type))
					{
						if (string.IsNullOrEmpty(num)) return null;
						parts.Add(new KeyValuePair<string, string>(num, type));
						num = string.Empty;
						type = string.Empty;
					}
					num += item;
				}
				else
					type += item;
			}
			if (string.IsNullOrEmpty(num)) return null;
			parts.Add(new KeyValuePair<string, string>(num, type));
			result = TimeSpan.Zero;
			foreach (KeyValuePair<string, string> item in parts)
			{
				double number = double.Parse(item.Key, NumberStyles.Float, NumberFormatInfo.InvariantInfo);
				switch (item.Value.ToLowerInvariant())
				{
					case "y":
					case "yr":
					case "year":
					case "years":
						result += TimeSpan.FromDays(365 * number);
						break;
					case "w":
					case "wk":
					case "week":
					case "weeks":
						result += TimeSpan.FromDays(7 * number);
						break;
					case "d":
					case "day":
					case "days":
						result += TimeSpan.FromDays(number);
						break;
					case "h":
					case "hr":
					case "hour":
					case "hours":
						result += TimeSpan.FromHours(number);
						break;
					case "m":
					case "min":
					case "minute":
					case "minutes":
						result += TimeSpan.FromMinutes(number);
						break;
					case "":
					case "s":
					case "sec":
					case "second":
					case "seconds":
						result += TimeSpan.FromSeconds(number);
						break;
					case "cs":
					case "centisecond":
					case "centiseconds":
						result += TimeSpan.FromMilliseconds(number * 10);
						break;
					case "f":
					case "frame":
					case "frames":
					case "ntsc":
					case "ntscframe":
					case "ntscframes":
						result += TimeSpan.FromTicks((long)(number * (TimeSpan.TicksPerSecond / 60.0)));
						break;
					case "pal":
					case "palframe":
					case "palframes":
						result += TimeSpan.FromTicks((long)(number * (TimeSpan.TicksPerSecond / 50.0)));
						break;
					case "ms":
					case "millisecond":
					case "milliseconds":
						result += TimeSpan.FromMilliseconds(number);
						break;
					case "tick":
					case "ticks":
						result += TimeSpan.FromTicks((long)number);
						break;
					default:
						return null;
				}
			}
			if (negative) result = -result;
			return result;
		}
	}
}
