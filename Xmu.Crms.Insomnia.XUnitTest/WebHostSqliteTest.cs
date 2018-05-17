using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;
using Xunit;
using static Xmu.Crms.Insomnia.XUnitTest.Utils;

namespace Xmu.Crms.Insomnia.XUnitTest
{
    public class WebHostSqliteTest
    {
        [Fact]
        public async Task CanCallFixGroup()
        {
            var basePath = GetProjectPath(Assembly.GetExecutingAssembly());
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            try
            {
                var server = (await connection.PopulateDbAsync(basePath)).MakeTestServer(basePath);
                using (var scope = server.Host.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var fg = services.GetRequiredService<IFixGroupService>();
                    fg.GetFixedGroupById(1, 1);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Fact]
        public async Task CanCallSeminarGroup()
        {
            var basePath = GetProjectPath(Assembly.GetExecutingAssembly());
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            try
            {
                var server = (await connection.PopulateDbAsync(basePath)).MakeTestServer(basePath);
                using (var scope = server.Host.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var sg = services.GetRequiredService<ISeminarGroupService>();
                    Assert.NotNull(sg.GetSeminarGroupByGroupId(1));
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Fact]
        public async Task CanGetEntityById()
        {
            var basePath = GetProjectPath(Assembly.GetExecutingAssembly());
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            try
            {
                var server = (await connection.PopulateDbAsync(basePath)).MakeTestServer(basePath);
                using (var scope = server.Host.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var db = services.GetRequiredService<CrmsContext>();
                    Assert.Single(await db.Attendences.Where(a => a.Id == 1).ToListAsync());
                    Assert.Single(await db.ClassInfo.Where(a => a.Id == 1).ToListAsync());
                    Assert.Single(await db.Course.Where(a => a.Id == 1).ToListAsync());
                    Assert.Single(await db.CourseSelection.Where(a => a.Id == 1).ToListAsync());
                    Assert.Single(await db.FixGroup.Where(a => a.Id == 1).ToListAsync());
                    Assert.Single(await db.FixGroupMember.Where(a => a.Id == 1).ToListAsync());
                    Assert.True((await db.Location.Where(a => a.Id == 1).ToListAsync()).Count <= 1);
                    Assert.Single(await db.Seminar.Where(a => a.Id == 1).ToListAsync());
                    Assert.Single(await db.SeminarGroup.Where(a => a.Id == 1).ToListAsync());
                    Assert.Single(await db.SeminarGroupMember.Where(a => a.Id == 1).ToListAsync());
                    Assert.Single(await db.SeminarGroupTopic.Where(a => a.Id == 1).ToListAsync());
                    Assert.Single(await db.StudentScoreGroup.Where(a => a.Id == 1).ToListAsync());
                    Assert.Single(await db.Topic.Where(a => a.Id == 1).ToListAsync());
                    Assert.Single(await db.UserInfo.Where(a => a.Id == 1).ToListAsync());
                    Assert.Single(await db.UserInfo.Where(a => a.Id == 3).ToListAsync());
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Fact]
        public async Task CanJoinFixGroup()
        {
            var basePath = GetProjectPath(Assembly.GetExecutingAssembly());
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            try
            {
                var server = (await connection.PopulateDbAsync(basePath)).MakeTestServer(basePath);
                using (var scope = server.Host.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var us = services.GetRequiredService<IUserService>();
                    var lg = services.GetRequiredService<ILoginService>();
                    var fg = services.GetRequiredService<IFixGroupService>();
                    var cr = services.GetRequiredService<ICourseService>();
                    var cl = services.GetRequiredService<IClassService>();
                    var stu = lg.SignUpPhone(new UserInfo {Phone = "18800002333", Password = "crms2017"});
                    us.UpdateUserByUserId(stu.Id,
                        new UserInfo
                        {
                            Name = "≤‚ ‘CSC",
                            Type = Type.Student,
                            Email = "a@b.test",
                            Gender = Gender.Male,
                            School = new School {Id = 1}
                        });
                    var classInfos = cr.ListClassByCourseName("øŒ≥Ã1");
                    var cls = classInfos.First();
                    cl.InsertCourseSelectionById(stu.Id, cls.Id);
                    Assert.NotEmpty(cl.ListClassByUserId(stu.Id));
                    var groups = fg.ListFixGroupByClassId(cls.Id);
                    var grp = groups.First();
                    fg.InsertStudentIntoGroup(stu.Id, grp.Id);
                    Assert.NotNull(fg.GetFixedGroupById(stu.Id, cls.Id));
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Fact]
        public async Task CanSelectCourse()
        {
            var basePath = GetProjectPath(Assembly.GetExecutingAssembly());
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            try
            {
                var server = (await connection.PopulateDbAsync(basePath)).MakeTestServer(basePath);
                using (var scope = server.Host.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var us = services.GetRequiredService<IUserService>();
                    var lg = services.GetRequiredService<ILoginService>();
                    var cr = services.GetRequiredService<ICourseService>();
                    var cl = services.GetRequiredService<IClassService>();
                    var stu = lg.SignUpPhone(new UserInfo {Phone = "18800002333", Password = "crms2017"});
                    us.UpdateUserByUserId(stu.Id,
                        new UserInfo
                        {
                            Name = "≤‚ ‘CSC",
                            Type = Type.Student,
                            Email = "a@b.test",
                            Gender = Gender.Male,
                            School = new School {Id = 1}
                        });
                    var classInfos = cr.ListClassByCourseName("øŒ≥Ã1");
                    cl.InsertCourseSelectionById(stu.Id, classInfos.First().Id);
                    Assert.NotEmpty(cl.ListClassByUserId(stu.Id));
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Fact]
        public async Task CanSelectTopic()
        {
            var basePath = GetProjectPath(Assembly.GetExecutingAssembly());
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            try
            {
                var server = (await connection.PopulateDbAsync(basePath)).MakeTestServer(basePath);
                using (var scope = server.Host.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var us = services.GetRequiredService<IUserService>();
                    var lg = services.GetRequiredService<ILoginService>();
                    var sg = services.GetRequiredService<ISeminarGroupService>();
                    var ss = services.GetRequiredService<ISeminarService>();
                    var cr = services.GetRequiredService<ICourseService>();
                    var cl = services.GetRequiredService<IClassService>();
                    var tp = services.GetRequiredService<ITopicService>();
                    var stu = lg.SignUpPhone(new UserInfo {Phone = "18800002333", Password = "crms2017"});
                    us.UpdateUserByUserId(stu.Id,
                        new UserInfo
                        {
                            Name = "≤‚ ‘CSC",
                            Type = Type.Student,
                            Email = "a@b.test",
                            Gender = Gender.Male,
                            School = new School {Id = 1}
                        });
                    var classInfos = cr.ListClassByCourseName("øŒ≥Ã1");
                    var cls = classInfos.First();
                    cl.InsertCourseSelectionById(stu.Id, cls.Id);
                    Assert.NotEmpty(cl.ListClassByUserId(stu.Id));
                    var seminars = ss.ListSeminarByCourseId(cls.Course.Id);
                    var sem = seminars.First();
                    var grps = sg.ListSeminarGroupBySeminarId(sem.Id);
                    var grp = grps.First();
                    sg.InsertSeminarGroupMemberById(stu.Id, grp.Id);
                    Assert.NotNull(sg.GetSeminarGroupById(sem.Id, stu.Id));
                    sg.ResignLeaderById(grp.Id, grp.Leader.Id);
                    sg.AssignLeaderById(grp.Id, stu.Id);
                    var topics = tp.ListTopicBySeminarId(sem.Id);
                    var trp = topics.First();
                    sg.InsertTopicByGroupId(grp.Id, trp.Id);
                }
            }
            finally
            {
                connection.Close();
            }
        }
    }
}