using System.Globalization;
using System.Windows;

namespace Pharmacy_Management_System.Helpers
{
    public static class AppLanguage
    {
        public static string CurrentLanguage { get; private set; } = "en";

        public static void SetLanguage(string languageCode)
        {
            CurrentLanguage = languageCode;

            try
            {
                if (Application.Current?.MainWindow == null) return;

                if (languageCode == "ps" || languageCode == "da" ||
                    languageCode == "ur" || languageCode == "ar")
                {
                    Application.Current.MainWindow.FlowDirection = FlowDirection.RightToLeft;
                }
                else
                {
                    Application.Current.MainWindow.FlowDirection = FlowDirection.LeftToRight;
                }
            }
            catch { }
        }

        public static string Get(string key)
        {
            return CurrentLanguage switch
            {
                "ps" => GetPashto(key),
                "da" => GetDari(key),
                "ur" => GetUrdu(key),
                "ar" => GetArabic(key),
                _ => GetEnglish(key)
            };
        }

        private static string GetEnglish(string key) => key switch
        {
            "Dashboard" => "Dashboard",
            "Medicines" => "Medicines",
            "Purchase" => "Purchase",
            "Sales" => "Sales",
            "Expenses" => "Expenses",
            "Employees" => "Employees",
            "Salary" => "Salary",
            "Debts" => "Debts",
            "Returns" => "Returns",
            "Accounts" => "Accounts",
            "Logs" => "Logs",
            "Reports" => "Reports",
            "Barcode" => "Barcode",
            "Settings" => "Settings",
            "TodaySales" => "Today's Sales",
            "TodayPurchase" => "Today's Purchase",
            "StockValue" => "Stock Value",
            "LowStock" => "Low Stock",
            "NearExpiry" => "Near Expiry",
            "ExpiredItems" => "Expired Items",
            "TotalMedicines" => "Total Medicines",
            "RecentSales" => "Recent Sales",
            "Alerts" => "Alerts",
            "AddMedicine" => "+ Add Medicine",
            "NewPurchase" => "+ New Purchase",
            "NewSale" => "+ New Sale",
            "AddExpense" => "+ Add Expense",
            "AddEmployee" => "+ Add Employee",
            "PaySalary" => "+ Pay Salary",
            "AddDebt" => "+ Add Debt",
            "AddReturn" => "+ Add Return",
            "AddAccount" => "+ Add Account",
            "Save" => "Save",
            "Cancel" => "Cancel",
            "Delete" => "Delete",
            "Edit" => "Edit",
            "Print" => "Print",
            "Search" => "Search...",
            "Welcome" => "Welcome Back!",
            "Login" => "Sign In to System",
            _ => key
        };

        private static string GetPashto(string key) => key switch
        {
            "Dashboard" => "کورپاڼه",
            "Medicines" => "درملونه",
            "Purchase" => "پیرودل",
            "Sales" => "خرڅول",
            "Expenses" => "لګښتونه",
            "Employees" => "کارکوونکي",
            "Salary" => "معاش",
            "Debts" => "قرضونه",
            "Returns" => "واپسي",
            "Accounts" => "حسابونه",
            "Logs" => "لاګونه",
            "Reports" => "راپورونه",
            "Barcode" => "بارکوډ",
            "Settings" => "تنظیمات",
            "TodaySales" => "د نن ورځ خرڅلاو",
            "TodayPurchase" => "د نن ورځ پیرودل",
            "StockValue" => "د سټاک ارزښت",
            "LowStock" => "لږ سټاک",
            "NearExpiry" => "د پای نیټې سره نږدې",
            "ExpiredItems" => "پای شوي توکي",
            "TotalMedicines" => "ټول درملونه",
            "RecentSales" => "وروستي خرڅلاوونه",
            "Alerts" => "خبرتیاوې",
            "AddMedicine" => "+ درمل اضافه کړئ",
            "NewPurchase" => "+ نوی پیرود",
            "NewSale" => "+ نوی خرڅلاو",
            "AddExpense" => "+ لګښت اضافه کړئ",
            "AddEmployee" => "+ کارکوونکی اضافه کړئ",
            "PaySalary" => "+ معاش ورکړئ",
            "AddDebt" => "+ قرض اضافه کړئ",
            "AddReturn" => "+ واپسي اضافه کړئ",
            "AddAccount" => "+ حساب اضافه کړئ",
            "Save" => "خوندي کړئ",
            "Cancel" => "لغوه کړئ",
            "Delete" => "ړنګ کړئ",
            "Edit" => "سمول",
            "Print" => "چاپ",
            "Search" => "لټون...",
            "Welcome" => "ښه راغلاست!",
            "Login" => "سیستم ته ننوتل",
            _ => key
        };

        private static string GetDari(string key) => key switch
        {
            "Dashboard" => "داشبورد",
            "Medicines" => "دواها",
            "Purchase" => "خرید",
            "Sales" => "فروش",
            "Expenses" => "مصارف",
            "Employees" => "کارمندان",
            "Salary" => "معاش",
            "Debts" => "قرض‌ها",
            "Returns" => "برگشتی",
            "Accounts" => "حساب‌ها",
            "Logs" => "گزارش‌ها",
            "Reports" => "راپورها",
            "Barcode" => "بارکد",
            "Settings" => "تنظیمات",
            "TodaySales" => "فروش امروز",
            "TodayPurchase" => "خرید امروز",
            "StockValue" => "ارزش سهام",
            "LowStock" => "موجودی کم",
            "NearExpiry" => "نزدیک به انقضا",
            "ExpiredItems" => "اقلام منقضی",
            "TotalMedicines" => "مجموع داروها",
            "RecentSales" => "فروش‌های اخیر",
            "Alerts" => "هشدارها",
            "AddMedicine" => "+ اضافه کردن دارو",
            "NewPurchase" => "+ خرید جدید",
            "NewSale" => "+ فروش جدید",
            "AddExpense" => "+ اضافه کردن مصرف",
            "AddEmployee" => "+ اضافه کردن کارمند",
            "PaySalary" => "+ پرداخت معاش",
            "AddDebt" => "+ اضافه کردن قرض",
            "AddReturn" => "+ اضافه کردن برگشتی",
            "AddAccount" => "+ اضافه کردن حساب",
            "Save" => "ذخیره",
            "Cancel" => "لغو",
            "Delete" => "حذف",
            "Edit" => "ویرایش",
            "Print" => "چاپ",
            "Search" => "جستجو...",
            "Welcome" => "خوش آمدید!",
            "Login" => "ورود به سیستم",
            _ => key
        };

        private static string GetUrdu(string key) => key switch
        {
            "Dashboard" => "ڈیش بورڈ",
            "Medicines" => "ادویات",
            "Purchase" => "خریداری",
            "Sales" => "فروخت",
            "Expenses" => "اخراجات",
            "Employees" => "ملازمین",
            "Salary" => "تنخواہ",
            "Debts" => "قرضے",
            "Returns" => "واپسی",
            "Accounts" => "حسابات",
            "Logs" => "لاگز",
            "Reports" => "رپورٹس",
            "Barcode" => "بارکوڈ",
            "Settings" => "ترتیبات",
            "TodaySales" => "آج کی فروخت",
            "TodayPurchase" => "آج کی خریداری",
            "StockValue" => "اسٹاک کی قیمت",
            "LowStock" => "کم اسٹاک",
            "NearExpiry" => "قریب المیعاد",
            "ExpiredItems" => "میعاد ختم اشیاء",
            "TotalMedicines" => "کل ادویات",
            "RecentSales" => "حالیہ فروخت",
            "Alerts" => "انتباہات",
            "AddMedicine" => "+ دوائی شامل کریں",
            "NewPurchase" => "+ نئی خریداری",
            "NewSale" => "+ نئی فروخت",
            "AddExpense" => "+ اخراج شامل کریں",
            "AddEmployee" => "+ ملازم شامل کریں",
            "PaySalary" => "+ تنخواہ دیں",
            "AddDebt" => "+ قرض شامل کریں",
            "AddReturn" => "+ واپسی شامل کریں",
            "AddAccount" => "+ حساب شامل کریں",
            "Save" => "محفوظ کریں",
            "Cancel" => "منسوخ",
            "Delete" => "حذف کریں",
            "Edit" => "ترمیم",
            "Print" => "پرنٹ",
            "Search" => "تلاش کریں...",
            "Welcome" => "خوش آمدید!",
            "Login" => "سسٹم میں داخل ہوں",
            _ => key
        };

        private static string GetArabic(string key) => key switch
        {
            "Dashboard" => "لوحة التحكم",
            "Medicines" => "الأدوية",
            "Purchase" => "الشراء",
            "Sales" => "المبيعات",
            "Expenses" => "المصروفات",
            "Employees" => "الموظفون",
            "Salary" => "الراتب",
            "Debts" => "الديون",
            "Returns" => "المرتجعات",
            "Accounts" => "الحسابات",
            "Logs" => "السجلات",
            "Reports" => "التقارير",
            "Barcode" => "الباركود",
            "Settings" => "الإعدادات",
            "TodaySales" => "مبيعات اليوم",
            "TodayPurchase" => "مشتريات اليوم",
            "StockValue" => "قيمة المخزون",
            "LowStock" => "مخزون منخفض",
            "NearExpiry" => "قريب من الانتهاء",
            "ExpiredItems" => "منتجات منتهية",
            "TotalMedicines" => "إجمالي الأدوية",
            "RecentSales" => "المبيعات الأخيرة",
            "Alerts" => "التنبيهات",
            "AddMedicine" => "+ إضافة دواء",
            "NewPurchase" => "+ شراء جديد",
            "NewSale" => "+ بيع جديد",
            "AddExpense" => "+ إضافة مصروف",
            "AddEmployee" => "+ إضافة موظف",
            "PaySalary" => "+ دفع راتب",
            "AddDebt" => "+ إضافة دين",
            "AddReturn" => "+ إضافة مرتجع",
            "AddAccount" => "+ إضافة حساب",
            "Save" => "حفظ",
            "Cancel" => "إلغاء",
            "Delete" => "حذف",
            "Edit" => "تعديل",
            "Print" => "طباعة",
            "Search" => "بحث...",
            "Welcome" => "مرحباً!",
            "Login" => "تسجيل الدخول",
            _ => key
        };
    }
}