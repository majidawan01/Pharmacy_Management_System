using System.Collections.Generic;
using System.Linq;

namespace Pharmacy_Management_System.Views
{
    public class Language
    {
        public string Code { get; set; }
        public string Flag { get; set; }
        public string Name { get; set; }
        public string WelcomeText { get; set; }
        public string UsernameLabel { get; set; }
        public string PasswordLabel { get; set; }
        public string SignInButton { get; set; }
        public string SelectLanguageLabel { get; set; }
        public string SubtitleText { get; set; }
        public string FlowDirection { get; set; }
    }

    public static class LanguageResources
    {
        public static Dictionary<string, Language> Languages { get; } = new Dictionary<string, Language>
        {
            {
                "en",
                new Language
                {
                    Code = "en",
                    Flag = "🇺🇸",
                    Name = "English",
                    WelcomeText = "Welcome Back! 👋",
                    UsernameLabel = "👤 Username",
                    PasswordLabel = "🔒 Password",
                    SignInButton = "🔑 Sign In to System",
                    SelectLanguageLabel = "Select language",
                    SubtitleText = "Please sign in to continue to Afghan Cosmos Pharmacy System",
                    FlowDirection = "LeftToRight"
                }
            },
            {
                "ps",
                new Language
                {
                    Code = "ps",
                    Flag = "🇦🇫",
                    Name = "پښتو",
                    WelcomeText = "ښه راغلاست! 👋",
                    UsernameLabel = "👤 کارن نوم",
                    PasswordLabel = "🔒 پټنوم",
                    SignInButton = "🔑 سیستم ته ننوتل",
                    SelectLanguageLabel = "ژبه غوره کړئ",
                    SubtitleText = "مهربانی کړئ د افغان کاسموس فارماسي سیستم کې داخل شئ",
                    FlowDirection = "RightToLeft"
                }
            },
            {
                "da",
                new Language
                {
                    Code = "da",
                    Flag = "🇦🇫",
                    Name = "دری",
                    WelcomeText = "خوش آمدید! 👋",
                    UsernameLabel = "👤 نام کاربری",
                    PasswordLabel = "🔒 رمز عبور",
                    SignInButton = "🔑 ورود به سیستم",
                    SelectLanguageLabel = "زبان را انتخاب کنید",
                    SubtitleText = "لطفاً به سیستم داروخانه افغان کاسموس وارد شوید",
                    FlowDirection = "RightToLeft"
                }
            },
            {
                "ur",
                new Language
                {
                    Code = "ur",
                    Flag = "🇵🇰",
                    Name = "اردو",
                    WelcomeText = "خوش آمدید! 👋",
                    UsernameLabel = "👤 صارف نام",
                    PasswordLabel = "🔒 پاس ورڈ",
                    SignInButton = "🔑 سسٹم میں داخل ہوں",
                    SelectLanguageLabel = "زبان منتخب کریں",
                    SubtitleText = "براہ کرم افغان کاسموس فارمیسی سسٹم میں داخل ہوں",
                    FlowDirection = "RightToLeft"
                }
            },
            {
                "ar",
                new Language
                {
                    Code = "ar",
                    Flag = "🇸🇦",
                    Name = "العربية",
                    WelcomeText = "مرحباً! 👋",
                    UsernameLabel = "👤 اسم المستخدم",
                    PasswordLabel = "🔒 كلمة المرور",
                    SignInButton = "🔑 تسجيل الدخول",
                    SelectLanguageLabel = "اختر اللغة",
                    SubtitleText = "يرجى تسجيل الدخول إلى نظام صيدلية أفغان كوزموس",
                    FlowDirection = "RightToLeft"
                }
            }
        };

        public static Language GetLanguage(string code)
        {
            return Languages.ContainsKey(code) ? Languages[code] : Languages["en"];
        }

        public static List<Language> GetAllLanguages()
        {
            return Languages.Values.ToList();
        }
    }
}
