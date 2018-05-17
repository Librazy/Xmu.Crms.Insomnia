using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xmu.Crms.Shared.Exceptions;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;

namespace Xmu.Crms.Services.Insomnia
{
    public class GroupService : ISeminarGroupService
    {
        private readonly CrmsContext _db;

        public GroupService(CrmsContext db) => _db = db;

        /// <inheritdoc />
        public void DeleteSeminarGroupMemberBySeminarGroupId(long seminarGroupId)
        {
            _db.RemoveRange(_db.SeminarGroupMember.Where(s => s.SeminarGroup.Id == seminarGroupId));
            _db.SaveChanges();
        }

        /// <inheritdoc />
        public long InsertSeminarGroupMemberById(long userId, long groupId)
        {
            if (userId < 0 && groupId < 0)
            {
                throw new ArgumentException();
            }

            var group = _db.SeminarGroup.SingleOrDefault(s => s.Id == groupId);
            if (group == null)
            {
                throw new GroupNotFoundException();
            }

            var student = _db.UserInfo.SingleOrDefault(u => u.Id == userId);

            if (student == null)
            {
                throw new UserNotFoundException();
            }

            var isExist = _db.SeminarGroupMember.Include(sg => sg.SeminarGroup).Include(sg => sg.Student)
                .Where(sg => sg.SeminarGroup.Id == groupId && sg.Student.Id == userId);
            if (isExist.Any())
            {
                throw new InvalidOperationException();
            }

            var seminargroup = _db.SeminarGroupMember.Add(new SeminarGroupMember
            {
                SeminarGroup = group,
                Student = student
            });
            _db.SaveChanges();

            return seminargroup.Entity.Id;
        }

        /// <inheritdoc />
        public IList<UserInfo> ListSeminarGroupMemberByGroupId(long groupId)
        {
            if (groupId < 0)
            {
                throw new ArgumentException();
            }

            var group = _db.SeminarGroup.SingleOrDefault(s => s.Id == groupId);
            if (group == null)
            {
                throw new GroupNotFoundException();
            }

            return _db.SeminarGroupMember
                .Include(s => s.Student)
                .Include(s => s.SeminarGroup)
                .Where(s => s.SeminarGroup.Id == groupId)
                .Select(s => s.Student)
                .ToList();
        }

        /// <inheritdoc />
        public IList<SeminarGroup> ListSeminarGroupIdByStudentId(long userId)
        {
            if (userId < 0)
            {
                throw new ArgumentException();
            }

            var user = _db.SeminarGroup.SingleOrDefault(u => u.Id == userId);
            if (user == null)
            {
                throw new GroupNotFoundException();
            }

            return _db.SeminarGroupMember.Include(s => s.Student).Include(s => s.SeminarGroup)
                .Where(s => s.Student.Id == userId)
                .Select(s => s.SeminarGroup).ToList();
        }

        /// <inheritdoc />
        public long GetSeminarGroupLeaderByGroupId(long groupId)
        {
            if (groupId < 0)
            {
                throw new ArgumentException();
            }

            var group = _db.SeminarGroup.Include(s => s.Leader).SingleOrDefault(s => s.Id == groupId);
            if (group == null)
            {
                throw new GroupNotFoundException();
            }

            return group.Leader.Id;
        }

        /// <inheritdoc />
        public IList<SeminarGroup> ListSeminarGroupBySeminarId(long seminarId)
        {
            if (seminarId < 0)
            {
                throw new ArgumentException();
            }

            var seminar = _db.Seminar.Find(seminarId) ?? throw new SeminarNotFoundException();
            return _db.SeminarGroup.Include(s => s.Seminar).Where(s => s.SeminarId == seminar.Id).ToList();
        }

        /// <inheritdoc />
        public void DeleteSeminarGroupBySeminarId(long seminarId)
        {
            if (seminarId < 0)
            {
                throw new ArgumentException();
            }

            _db.SeminarGroup.RemoveRange(_db.SeminarGroup.Where(s => s.Seminar.Id == seminarId));
            _db.SaveChanges();
        }

        /// <inheritdoc />
        public long InsertSeminarGroupBySeminarId(long seminarId, long classId, SeminarGroup seminarGroup)
        {
            if (seminarId < 0)
            {
                throw new ArgumentException();
            }

            var seminarinfo = _db.Seminar.Find(seminarId) ?? throw new SeminarNotFoundException();
            var classinfo = _db.ClassInfo.Find(classId) ?? throw new ClassNotFoundException();
            seminarGroup.Seminar = seminarinfo;
            seminarGroup.ClassInfo = classinfo;
            var group = _db.SeminarGroup.Add(seminarGroup);
            _db.SaveChanges();
            return group.Entity.Id;
        }

        /// <inheritdoc />
        public long InsertSeminarGroupMemberByGroupId(long groupId, SeminarGroupMember seminarGroupMember)
        {
            if (groupId < 0)
            {
                throw new ArgumentException();
            }

            var group = _db.SeminarGroup.Find(groupId);
            seminarGroupMember.SeminarGroup = group;
            var member = _db.SeminarGroupMember.Add(seminarGroupMember);
            _db.SaveChanges();
            return member.Entity.Id;
        }

        /// <inheritdoc />
        public void DeleteSeminarGroupByGroupId(long seminarGroupId)
        {
            if (seminarGroupId < 0)
            {
                throw new ArgumentException();
            }

            _db.SeminarGroup.RemoveRange(_db.SeminarGroup.Where(s => s.Id == seminarGroupId));
            _db.SaveChanges();
        }

        /// <inheritdoc />
        public SeminarGroup GetSeminarGroupByGroupId(long groupId)
        {
            if (groupId < 0)
            {
                throw new ArgumentException();
            }

            var group = _db.SeminarGroup.Find(groupId);
            if (group == null)
            {
                throw new GroupNotFoundException();
            }

            return group;
        }

        /// <inheritdoc />
        public long GetSeminarGroupLeaderById(long userId, long seminarId)
        {
            if (userId < 0 || seminarId < 0)
            {
                throw new ArgumentException();
            }

            var seminarmember = _db.SeminarGroupMember
                .Include(s => s.Student)
                .Include(s => s.SeminarGroup)
                .ThenInclude(sem => sem.Seminar)
                .Where(s => s.Student.Id == userId)
                .SingleOrDefault(sg => sg.SeminarGroup.Seminar.Id == seminarId);
            if (seminarmember != null)
            {
                return seminarmember.SeminarGroup.Leader.Id;
            }

            return -1;
        }

        /// <inheritdoc />
        public void AutomaticallyGrouping(long seminarId, long classId)
        {
            if (seminarId < 0 || classId < 0)
            {
                throw new ArgumentException();
            }

            var seminar = _db.Seminar.Find(seminarId);
            if (seminar == null)
            {
                throw new SeminarNotFoundException();
            }

            var classes = _db.ClassInfo.Find(classId);
            if (classes == null)
            {
                throw new ClassNotFoundException();
            }

            var members = new List<UserInfo>();
            _db.CourseSelection.Where(c => c.ClassInfo.Id == classId)
                .Select(c => c.Student)
                .ToList().ForEach(member => members.Add(member));
            var count = 0;
            UserInfo[] memArrays = { };
            members.ForEach(member => memArrays[count++] = member);
            var looptime = memArrays.Length / 2;
            var tick = DateTime.Now.Ticks;
            var ran = new Random((int) (tick & 0xffffffffL) | (int) (tick >> 32));
            while (looptime >= 0)
            {
                var ran1 = ran.Next(0, memArrays.Length);
                var ran2 = ran.Next(0, memArrays.Length);
                var temp = memArrays[ran1];
                memArrays[ran1] = memArrays[ran2];
                memArrays[ran2] = temp;
                looptime--;
            }

            var countgroup = memArrays.Length / 5 + 1;
            for (var i = 0; i < countgroup; i++)
            {
                _db.SeminarGroup.Add(new SeminarGroup
                {
                    Seminar = seminar,
                    ClassInfo = classes
                });
                var group = _db.SeminarGroup.Where(s => s.Seminar.Id == seminarId)
                    .SingleOrDefault(s => s.ClassInfo.Id == classId);
                for (var j = 0; j < 5; j++)
                {
                    var usertemp = memArrays[i * 5 + j];
                    _db.SeminarGroupMember.Add(new SeminarGroupMember
                    {
                        SeminarGroup = group,
                        Student = usertemp
                    });
                }
            }

            _db.SaveChanges();
        }

        /// <inheritdoc />
        public SeminarGroup GetSeminarGroupById(long seminarId, long userId)
        {
            if (userId < 0 || seminarId < 0)
            {
                throw new ArgumentException();
            }

            var seminar = _db.Seminar.Find(seminarId);
            if (seminar == null)
            {
                throw new SeminarNotFoundException();
            }

            var user = _db.UserInfo.Find(userId);
            if (user == null)
            {
                throw new UserNotFoundException();
            }

            var seminarmember = _db.SeminarGroupMember.Include(s => s.Student).Include(s => s.SeminarGroup)
                .ThenInclude(sem => sem.Seminar).Where(s => s.Student == user)
                .SingleOrDefault(sg => sg.SeminarGroup.Seminar == seminar);
            if (seminarmember == null)
            {
                throw new InvalidOperationException();
            }

            return seminarmember.SeminarGroup;
        }

        /// <inheritdoc />
        public IList<SeminarGroup> ListGroupByTopicId(long topicId)
        {
            if (topicId < 0)
            {
                throw new ArgumentException();
            }

            _db.SaveChanges();
            return _db.SeminarGroupTopic.Include(s => s.Topic).Include(s => s.SeminarGroup)
                .Where(s => s.Topic.Id == topicId)
                .Select(sg => sg.SeminarGroup)
                .ToList();
        }

        /// <inheritdoc />
        public string InsertTopicByGroupId(long groupId, long topicId)
        {
            if (groupId < 0 || topicId < 0)
            {
                throw new ArgumentException();
            }

            var group = _db.SeminarGroup.Find(groupId);
            if (group == null)
            {
                throw new GroupNotFoundException();
            }

            var topic = _db.Topic.Find(topicId);
            _db.SeminarGroupTopic.Add(new SeminarGroupTopic
            {
                Topic = topic,
                SeminarGroup = group
            });
            _db.SaveChanges();
            return null;
        }

        /// <inheritdoc />
        public void AssignLeaderById(long groupId, long userId)
        {
            if (groupId < 0 || userId < 0)
            {
                throw new ArgumentException();
            }

            var user = _db.UserInfo.Find(userId) ?? throw new UserNotFoundException();
            var group = _db.SeminarGroup.Include(s => s.Leader).SingleOrDefault(s => s.Id == groupId) ??
                        throw new GroupNotFoundException();
            if (group.Leader != null)
            {
                throw new InvalidOperationException();
            }

            group.Leader = user;
            _db.SaveChanges();
        }

        /// <inheritdoc />
        public void ResignLeaderById(long groupId, long userId)
        {
            if (groupId < 0 || userId < 0)
            {
                throw new ArgumentException();
            }

            var user = _db.UserInfo.Find(userId);
            if (user == null)
            {
                throw new UserNotFoundException();
            }

            var group = _db.SeminarGroup.Include(sg => sg.Leader).SingleOrDefault(s => s.Id == groupId);
            if (group == null)
            {
                throw new GroupNotFoundException();
            }

            if (group.Leader != user)
            {
                throw new InvalidOperationException();
            }

            group.Leader = null;
            _db.SaveChanges();
        }
    }
}