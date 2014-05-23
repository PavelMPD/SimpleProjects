using System.Diagnostics;
using System.Linq;
using MyDiary.Common;
using MyDiary.Model;
using MyDiary.DataAccess;
using NUnit.Framework;

namespace MyDiary.FunctionalTests.Data_Access
{
    public class DataAccessTest : BaseTest
    {
        [Test]
        public void InsertUserTest()
        {
            using (var dataContext = new DataContext())
            {
                var userRepository = new Repository<User>();
                var newUser = new User() {FirstName = "Юрий", LastName = "Иванов"};
                userRepository.Save(newUser);
                Trace.WriteLine(userRepository.Get(u => u.Id == newUser.Id).SingleOrDefault().PropertiesToString());
            }
        }
    }
}
