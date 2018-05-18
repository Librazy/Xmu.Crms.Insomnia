using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xmu.Crms.Shared.Exceptions;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;
using static Xmu.Crms.Services.Insomnia.PasswordUtils;


namespace Xmu.Crms.Services.Insomnia
{
    public class Pbkdf2LoginService : ILoginService
    {
        private readonly CrmsContext _db;

        public Pbkdf2LoginService(CrmsContext db) => _db = db;

        // .Net 平台不需要实现
        /// <inheritdoc />
        public UserInfo SignInWeChat(long userId, string code, string state, string successUrl) =>
            throw new NotImplementedException();

        /// <inheritdoc />
        public async Task<UserInfo> SignInPhoneAsync(UserInfo user)
        {
            var userInfo = await _db.UserInfo.SingleOrDefaultAsync(u => u.Phone == user.Phone) ??
                           throw new UserNotFoundException();
            if (IsExpectedPassword(user.Password, ReadHashString(userInfo.Password)))
            {
                return userInfo;
            }

            throw new PasswordErrorException();
        }

        /// <inheritdoc />
        public async Task<UserInfo> SignUpPhoneAsync(UserInfo user)
        {
            user.Password = HashString(user.Password);
            if (_db.UserInfo.Any(u => u.Phone == user.Phone))
            {
                throw new PhoneAlreadyExistsException();
            }

            var entry = _db.UserInfo.Add(user);
            await _db.SaveChangesAsync();
            return entry.Entity;
        }

        // .Net 平台不需要实现
        /// <inheritdoc />
        public void DeleteTeacherAccount(long userId) => throw new NotImplementedException();

        // .Net 平台不需要实现
        /// <inheritdoc />
        public void DeleteStudentAccount(long userId) => throw new NotImplementedException();
    }
}