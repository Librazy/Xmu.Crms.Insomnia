using System;
using System.Collections.Generic;
using System.Linq;
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
    public class UserService : Orleans.Grain, IUserService
    {
        private readonly CrmsContext _db;

        public UserService(CrmsContext db) => _db = db;

        /// <inheritdoc />
        public async Task InsertAttendanceByIdAsync(long classId, long seminarId, long userId, double longitude,
            double latitude)
        {
            var usr = await GetUserByUserIdAsync(userId);
            var cls = _db.ClassInfo.Find(classId) ?? throw new ClassNotFoundException();
            var sem = _db.Seminar.Find(seminarId) ?? throw new SeminarNotFoundException();
            var loc = _db.Location.Include(a => a.ClassInfo).Include(a => a.Seminar)
                          .SingleOrDefault(l => l.Seminar == sem && l.ClassInfo == cls) ??
                      throw new InvalidOperationException();
            _db.Attendences.Add(new Attendance
            {
                AttendanceStatus = loc.Status == 1 ? AttendanceStatus.Present : AttendanceStatus.Late,
                ClassInfo = cls,
                Seminar = sem,
                Student = usr
            });
            await _db.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<IList<Attendance>> ListAttendanceByIdAsync(long classId, long seminarId)
        {
            var cls = _db.ClassInfo.Find(classId) ?? throw new ClassNotFoundException();
            var sem = _db.Seminar.Find(seminarId) ?? throw new SeminarNotFoundException();
            return await _db.Attendences.Include(a => a.ClassInfo).Include(a => a.Seminar)
                .Where(a => a.ClassInfo == cls && a.Seminar == sem).ToListAsync();
        }

        /// <inheritdoc />
        public async Task<UserInfo> GetUserByUserIdAsync(long userId) => await _db.UserInfo.FindAsync(userId) ??
                                                        throw new UserNotFoundException();

        /// <inheritdoc />
        public Task<UserInfo> GetUserByUserNumberAsync(string userNumber)
        {
            return _db.UserInfo.SingleOrDefaultAsync(u => u.Number == userNumber) ??
                   throw new UserNotFoundException();
        }

        /// <inheritdoc />
        public async Task<IList<long>> ListUserIdByUserNameAsync(string userName)
        {
            return await _db.UserInfo.Where(u => u.Name.StartsWith(userName)).Select(u => u.Id).ToListAsync();
        }

        /// <inheritdoc />
        public async Task UpdateUserByUserIdAsync(long userId, UserInfo user)
        {
            var usr = await GetUserByUserIdAsync(userId);
            usr.Name = user.Name;
            usr.Avatar = user.Avatar;
            usr.Education = user.Education ?? Education.Bachelor;
            usr.Email = user.Email;
            usr.Gender = user.Gender;
            if (user.School != null)
            {
                usr.School = _db.School.Find(user.School.Id);
            }

            if ((user.SchoolId ?? 0) != 0)
            {
                usr.School = _db.School.Find(user.SchoolId);
            }

            usr.Title = user.Title ?? Title.Professer;
            if (usr.Type == Type.Unbinded)
            {
                usr.Type = user.Type;
                usr.Number = user.Number;
            }
            else if (user.Type != null && usr.Type != user.Type)
            {
                throw new InvalidOperationException();
            }

            _db.SaveChanges();
        }

        /// <inheritdoc />
        public async Task<IList<UserInfo>> ListUserByClassIdAsync(long classId, string numBeginWith, string nameBeginWith)
        {
            var cls = _db.ClassInfo.Find(classId) ?? throw new ClassNotFoundException();
            var userInfos = _db.CourseSelection.Include(c => c.ClassInfo).Include(c => c.Student)
                .Where(c => c.ClassInfo == cls).Select(c => c.Student);
            if (!string.IsNullOrEmpty(nameBeginWith))
            {
                userInfos = userInfos.Where(u => u.Name.StartsWith(nameBeginWith));
            }

            if (!string.IsNullOrEmpty(nameBeginWith))
            {
                userInfos = userInfos.Where(u => u.Number.StartsWith(numBeginWith));
            }

            return await userInfos.ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IList<UserInfo>> ListUserByUserNameAsync(string userName)
        {
            return await _db.UserInfo.Where(u => u.Name.StartsWith(userName)).ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IList<UserInfo>> ListPresentStudentAsync(long seminarId, long classId)
        {
            var cls = _db.ClassInfo.Find(classId) ?? throw new ClassNotFoundException();
            var sem = _db.Seminar.Find(seminarId) ?? throw new SeminarNotFoundException();
            return await _db.Attendences.Include(a => a.ClassInfo).Include(a => a.Seminar).Where(a =>
                    a.ClassInfo == cls && a.Seminar == sem && a.AttendanceStatus == AttendanceStatus.Present)
                .Select(a => a.Student).ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IList<UserInfo>> ListLateStudentAsync(long seminarId, long classId)
        {
            var cls = _db.ClassInfo.Find(classId) ?? throw new ClassNotFoundException();
            var sem = _db.Seminar.Find(seminarId) ?? throw new SeminarNotFoundException();
            return await _db.Attendences.Include(a => a.ClassInfo).Include(a => a.Seminar).Where(a =>
                    a.ClassInfo == cls && a.Seminar == sem && a.AttendanceStatus == AttendanceStatus.Late)
                .Select(a => a.Student).ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IList<UserInfo>> ListAbsenceStudentAsync(long seminarId, long classId)
        {
            var cls = _db.ClassInfo.Find(classId) ?? throw new ClassNotFoundException();
            var sem = _db.Seminar.Find(seminarId) ?? throw new SeminarNotFoundException();
            return await _db.Attendences.Include(a => a.ClassInfo).Include(a => a.Seminar).Where(a =>
                    a.ClassInfo == cls && a.Seminar == sem && a.AttendanceStatus == AttendanceStatus.Absent)
                .Select(a => a.Student).ToListAsync();
        }
    }
}