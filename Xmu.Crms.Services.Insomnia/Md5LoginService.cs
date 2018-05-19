using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Orleans.Concurrency;
using Xmu.Crms.Shared.Exceptions;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;
using Type = Xmu.Crms.Shared.Models.Type;

namespace Xmu.Crms.Services.Insomnia
{
    [StatelessWorker]
    public class Md5LoginService : Orleans.Grain, ILoginService
    {
        private readonly CrmsContext _db;

        public Md5LoginService(CrmsContext db) => _db = db;

        // .Net 平台不需要实现
        /// <inheritdoc />
        public Task<UserInfo> SignInWeChatAsync(long userId, string code, string state, string successUrl) =>
            throw new NotImplementedException();

        /// <inheritdoc />
        public async Task<UserInfo> SignInPhoneAsync(UserInfo user)
        {
            var userInfo = await _db.UserInfo.SingleOrDefaultAsync(u => u.Phone == user.Phone) ??
                           throw new UserNotFoundException();
            if (GetMd5(user.Password) == userInfo.Password)
            {
                return userInfo;
            }

            throw new PasswordErrorException();
        }

        /// <inheritdoc />
        public async Task<UserInfo> SignUpPhoneAsync(UserInfo user)
        {
            user.Password = GetMd5(user.Password);
            if (_db.UserInfo.Any(u => u.Phone == user.Phone))
            {
                throw new PhoneAlreadyExistsException();
            }

            user.Type = Type.Unbinded;

            var entry = _db.UserInfo.Add(user);
            await _db.SaveChangesAsync();
            return entry.Entity;
        }

        // .Net 平台不需要实现
        /// <inheritdoc />
        public Task DeleteTeacherAccountAsync(long userId) => throw new NotImplementedException();

        // .Net 平台不需要实现
        /// <inheritdoc />
        public Task DeleteStudentAccountAsync(long userId) => throw new NotImplementedException();

        private static string GetMd5(string strPwd)
        {
            using (var md5 = MD5.Create())
            {
                var byteHash = md5.ComputeHash(Encoding.UTF8.GetBytes(strPwd));
                var strRes = BitConverter.ToString(byteHash).Replace("-", "");
                strRes = strRes.ToUpper();
                return strRes.Length > 24 ? strRes.Substring(8, 16) : strRes;
            }
        }
    }
}