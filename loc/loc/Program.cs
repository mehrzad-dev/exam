using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text;

// فعال‌کردن UTF-8 برای ورودی و خروجی فارسی
Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

// اتصال به فایل‌های Resource پروژه
var resources = new ResourceManager(
    "LocalizationChallenge.Messages",
    Assembly.GetExecutingAssembly());

// دریافت پیام انتخاب زبان از Resource پیش‌فرض
Console.WriteLine(resources.GetString(
    "SelectLanguage", CultureInfo.InvariantCulture));

string input = Console.ReadLine()?.Trim().ToLowerInvariant() ?? "";

CultureInfo culture;

try
{
    // تبدیل زبان ورودی به Culture مناسب
    // امکان دریافت زبان‌های دیگر نیز وجود دارد.
    culture = CultureInfo.CreateSpecificCulture(input switch
    {
        "fa" => "fa-IR",
        "en" or "" => "en-US",
        _ => input
    });
}
catch (CultureNotFoundException)
{
    Console.WriteLine(resources.GetString(
        "InvalidCulture", CultureInfo.InvariantCulture));
    return;
}

// تنظیم Culture برای نمایش صحیح تاریخ، عدد و ارز
CultureInfo.CurrentCulture = culture;

// تنظیم زبان برای انتخاب فایل Resource مناسب
CultureInfo.CurrentUICulture = culture;

// دریافت متن ترجمه‌شده بر اساس کلید
string T(string key) => resources.GetString(key) ?? key;

void Print(string text)
{
    // تعیین جهت متن بدون معکوس کردن حروف و اعداد
    string direction = culture.TextInfo.IsRightToLeft
        ? "\u2067"
        : "\u2066";

    Console.WriteLine(direction + text + "\u2069");
}

// نمایش پیام‌ها، تاریخ، عدد و ارز متناسب با زبان
Print(T("Welcome"));
Print($"{T("Date")}: {DateTime.Now:D}");
Print($"{T("Number")}: {12345.67:N2}");
Print($"{T("Currency")}: {12345.67m:C}");