using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xmu.Crms.Shared.Exceptions;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;

namespace Xmu.Crms.Services.Insomnia
{
    public class FixedGroupService : IFixGroupService
    {
        private readonly CrmsContext _db;

        public FixedGroupService(CrmsContext db) => _db = db;

        /// <inheritdoc />
        public async Task<long> InsertFixGroupByClassIdAsync(long classId, long userId)
        {
            if (classId <= 0)
            {
                throw new ArgumentException(nameof(classId));
            }

            if (userId <= 0)
            {
                throw new ArgumentException(nameof(userId));
            }

            var cls = _db.ClassInfo.Find(classId) ?? throw new ClassNotFoundException();
            var usr = _db.UserInfo.Find(userId) ?? throw new UserNotFoundException();
            var fg = _db.FixGroup.Add(new FixGroup {ClassInfo = cls, Leader = usr});
            await _db.SaveChangesAsync();
            return fg.Entity.Id;
        }

        /// <inheritdoc />
        public async Task DeleteFixGroupMemberByFixGroupIdAsync(long fixGroupId)
        {
            if (fixGroupId <= 0)
            {
                throw new ArgumentException(nameof(fixGroupId));
            }

            _db.FixGroupMember.RemoveRange(_db.FixGroupMember.Include(m => m.FixGroup)
                .Where(m => m.FixGroup.Id == fixGroupId));
            await _db.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<IList<UserInfo>> ListFixGroupMemberByGroupIdAsync(long groupId)
        {
            if (groupId <= 0)
            {
                throw new ArgumentException(nameof(groupId));
            }

            var fixGroup = await _db.FixGroup.FindAsync(groupId) ?? throw new FixGroupNotFoundException();
            return await _db.FixGroupMember.Include(f => f.FixGroup).Include(f => f.Student)
                .Where(f => f.FixGroup == fixGroup).Select(f => f.Student).ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IList<FixGroup>> ListFixGroupByClassIdAsync(long classId)
        {
            if (classId <= 0)
            {
                throw new ArgumentException(nameof(classId));
            }

            var cls = await _db.ClassInfo.FindAsync(classId) ?? throw new ClassNotFoundException();
            return await _db.FixGroup.Include(f => f.ClassInfo).Where(f => f.ClassInfo == cls).ToListAsync();
        }

        /// <inheritdoc />
        public async Task DeleteFixGroupByClassIdAsync(long classId)
        {
            if (classId <= 0)
            {
                throw new ArgumentException(nameof(classId));
            }

            var cls = _db.ClassInfo.Find(classId) ?? throw new ClassNotFoundException();
            var members = _db.FixGroupMember.Include(f => f.FixGroup).ThenInclude(f => f.ClassInfo)
                .Where(f => f.FixGroup.ClassInfo == cls);
            var fixGroups = members.Select(m => m.FixGroup).Distinct();
            _db.FixGroupMember.RemoveRange(members);
            _db.FixGroup.RemoveRange(fixGroups);
            await _db.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task DeleteFixGroupByGroupIdAsync(long groupId)
        {
            if (groupId <= 0)
            {
                throw new ArgumentException(nameof(groupId));
            }

            await DeleteFixGroupMemberByFixGroupIdAsync(groupId);
            _db.Remove(await _db.FixGroup.FindAsync(groupId) ?? throw new FixGroupNotFoundException());
            await _db.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task UpdateFixGroupByGroupIdAsync(long groupId, FixGroup fixGroupBo)
        {
            if (groupId <= 0)
            {
                throw new ArgumentException(nameof(groupId));
            }

            var fixGroup = await _db.FixGroup.FindAsync(groupId) ?? throw new FixGroupNotFoundException();
            fixGroup.ClassInfo = fixGroupBo.ClassInfo;
            fixGroup.Leader = fixGroupBo.Leader;
            await _db.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<long> InsertStudentIntoGroupAsync(long userId, long groupId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException(nameof(userId));
            }

            if (groupId <= 0)
            {
                throw new ArgumentException(nameof(groupId));
            }

            var fixGroup = await _db.FixGroup.FindAsync(groupId) ?? throw new FixGroupNotFoundException();
            var entry = _db.FixGroupMember.Add(new FixGroupMember
            {
                FixGroup = fixGroup,
                Student = await _db.UserInfo.FindAsync(userId) ?? throw new UserNotFoundException()
            });
            await _db.SaveChangesAsync();
            return entry.Entity.Id;
        }

        /// <inheritdoc />
        public async Task<FixGroup> GetFixedGroupByIdAsync(long userId, long classId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException(nameof(userId));
            }

            if (classId <= 0)
            {
                throw new ArgumentException(nameof(classId));
            }

            var usr = await _db.UserInfo.FindAsync(userId) ?? throw new UserNotFoundException();
            var cls = await _db.ClassInfo.FindAsync(classId) ?? throw new ClassNotFoundException();
            var fixGroup = await _db.FixGroupMember.Include(m => m.FixGroup)
                .Where(m => m.StudentId == usr.Id && m.FixGroup.ClassId == cls.Id).Select(m => m.FixGroup)
                .SingleOrDefaultAsync();
            if (fixGroup != null)
            {
                return fixGroup;
            }

            fixGroup = await _db.FixGroup.FindAsync(InsertFixGroupByClassIdAsync(classId, userId));
            await InsertStudentIntoGroupAsync(userId, fixGroup.Id);

            return fixGroup;
        }

        /// <inheritdoc />
        public async Task DeleteFixGroupUserByIdAsync(long fixGroupId, long userId)
        {
            var grp = await _db.FixGroup.FindAsync(fixGroupId) ?? throw new GroupNotFoundException();
            var usr = await _db.UserInfo.FindAsync(userId) ?? throw new UserNotFoundException();
            _db.FixGroupMember.RemoveRange(_db.FixGroupMember.Include(f => f.FixGroup)
                .Where(f => f.FixGroup == grp && f.Student == usr));
            await _db.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<IList<FixGroupMember>> ListFixGroupByGroupIdAsync(long groupId)
        {
            var grp = await _db.FixGroup.FindAsync(groupId) ?? throw new GroupNotFoundException();
            return await _db.FixGroupMember.Include(f => f.FixGroup).Where(f => f.FixGroup == grp).ToListAsync();
        }
    }
}