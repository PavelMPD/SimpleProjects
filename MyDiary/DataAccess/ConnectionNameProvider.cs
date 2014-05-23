using System;

namespace MyDiary.DataAccess
{
    public static class ConnectionNameProvider
    {
        static ConnectionNameProvider()
        {
            ConnectionName = "MyDiaryConnectionString";
        }

        public static String ConnectionName { get; set; }
    }
}
