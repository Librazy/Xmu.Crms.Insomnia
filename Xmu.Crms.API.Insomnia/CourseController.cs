using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orleans;
using Xmu.Crms.Shared.Exceptions;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;
using Type = Xmu.Crms.Shared.Models.Type;

namespace Xmu.Crms.Insomnia
{
    [Route("")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CourseController : Controller
    {
        private readonly IClassService _classService;
        private readonly ICourseService _courseService;

        private readonly CrmsContext _db;
        private readonly ISeminarGroupService _seminarGroupService;
        private readonly ISeminarService _seminarService;
        private readonly IUserService _userService;

        public CourseController(IClusterClient client, CrmsContext db)
        {
            _courseService = client.GetGrain<ICourseService>(0);
            _classService = client.GetGrain<IClassService>(0);
            _userService = client.GetGrain<IUserService>(0);
            _seminarGroupService = client.GetGrain<ISeminarGroupService>(0);
            _seminarService = client.GetGrain<ISeminarService>(0);
            _db = db;
        }

        /*
         * 无法计算每个课程里面学生的人数，需要多表联合查询，查询难度非常大
         * 缺少班级总人数字段
         */
        [HttpGet("/course")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "TEACHER")]
        public async Task<IActionResult> GetUserCourses()
        {
            var userlogin = await _userService.GetUserByUserIdAsync(User.Id());
            if (userlogin.Type != Type.Teacher)
            {
                return StatusCode(403, new {msg = "权限不足"});
            }

            var courses = await _courseService.ListCourseByUserIdAsync(User.Id());
            return Json(courses.Select(c => new
            {
                id = c.Id,
                name = c.Name,
                numClass = _classService.ListClassByCourseIdAsync(c.Id).Result.Count,
                numStudent = _classService.ListClassByCourseIdAsync(c.Id).Result.Aggregate(0,
                    (total, cls) => _db.Entry(cls).Collection(cl => cl.CourseSelections).Query().Count() + total),
                startTime = c.StartDate.ToString("yyyy-MM-dd"),
                endTime = c.EndDate.ToString("yyyy-MM-dd")
            }));
        }

        [HttpPost("/course")]
        public async Task<IActionResult> CreateCourse([FromBody] CourseWithProportions newCourse)
        {
            var userlogin = await _userService.GetUserByUserIdAsync(User.Id());
            if (userlogin.Type != Type.Teacher)
            {
                return StatusCode(403, new {msg = "权限不足"});
            }

            var id = _courseService.InsertCourseByUserIdAsync(User.Id(), new Course
            {
                Name = newCourse.Name,
                Description = newCourse.Description,
                StartDate = newCourse.StartTime,
                EndDate = newCourse.EndTime,
                ThreePointPercentage = newCourse.Proportions.C,
                FourPointPercentage = newCourse.Proportions.B,
                FivePointPercentage = newCourse.Proportions.A,
                ReportPercentage = newCourse.Proportions.Report,
                PresentationPercentage = newCourse.Proportions.Presentation,
                TeacherId = User.Id()
            });
            return Created($"/course/{id}", new {id});
        }

        [HttpGet("/course/{courseId:long}")]
        public async Task<IActionResult> GetCourseById([FromRoute] long courseId)
        {
            try
            {
                var course = await _courseService.GetCourseByCourseIdAsync(courseId);
                var result = Json(new
                {
                    id = course.Id,
                    name = course.Name,
                    description = course.Description,
                    teacherName = course.Teacher.Name,
                    teacherEmail = course.Teacher.Email
                });
                return result;
            }
            catch (CourseNotFoundException)
            {
                return StatusCode(404, new {msg = "未找到课程"});
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new {msg = "错误的ID格式"});
            }
        }

        [HttpDelete("/course/{courseId:long}")]
        public async Task<IActionResult> DeleteCourseById([FromRoute] long courseId)
        {
            try
            {
                var userlogin = await _userService.GetUserByUserIdAsync(User.Id());
                if (userlogin.Type == Type.Teacher)
                {
                    await _courseService.DeleteCourseByCourseIdAsync(courseId);
                    return NoContent();
                }

                return StatusCode(403, new {msg = "权限不足"});
            }
            catch (CourseNotFoundException)
            {
                return StatusCode(404, new {msg = "未找到课程"});
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new {msg = "课程ID格式错误"});
            }
        }

        [HttpPut("/course/{courseId:long}")]
        public async Task<IActionResult> UpdateCourseById([FromRoute] long courseId, [FromBody] Course updated)
        {
            try
            {
                var userlogin = await _userService.GetUserByUserIdAsync(User.Id());
                if (userlogin.Type == Type.Teacher)
                {
                    await _courseService.UpdateCourseByCourseIdAsync(courseId, updated);
                    return NoContent();
                }

                return StatusCode(403, new {msg = "权限不足"});
            }
            catch (CourseNotFoundException)
            {
                return StatusCode(404, new {msg = "未找到课程"});
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new {msg = "课程ID格式错误"});
            }
        }

        [HttpGet("/course/{courseId:long}/class")]
        public async Task<IActionResult> GetClassesByCourseId([FromRoute] long courseId)
        {
            try
            {
                var classes = await _classService.ListClassByCourseIdAsync(courseId);
                return Json(classes.Select(c => new
                {
                    id = c.Id,
                    name = c.Name
                }));
            }
            catch (CourseNotFoundException)
            {
                return StatusCode(404, new {msg = "未找到课程"});
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new {msg = "课程ID格式错误"});
            }
        }

        [HttpPost("/course/{courseId:long}/class")]
        public async Task<IActionResult> CreateClassByCourseId([FromRoute] long courseId, [FromBody] ClassWithProportions newClass)
        {
            var userlogin = await _userService.GetUserByUserIdAsync(User.Id());
            if (userlogin.Type != Type.Teacher)
            {
                return StatusCode(403, new {msg = "权限不足"});
            }

            var classId = _courseService.InsertClassByIdAsync(courseId, new ClassInfo
            {
                Name = newClass.Name,
                ClassTime = newClass.Time,
                Site = newClass.Site,
                ThreePointPercentage = newClass.Proportions.C,
                FourPointPercentage = newClass.Proportions.B,
                FivePointPercentage = newClass.Proportions.A,
                ReportPercentage = newClass.Proportions.Report,
                PresentationPercentage = newClass.Proportions.Presentation
            });
            return Created($"/class/{classId}", new {id = classId});
        }

        /*
         * 这里新增了一个FromBody的embededGrade的参数，用于判断是否已经打分
         */
        [HttpGet("/course/{courseId:long}/seminar")]
        public async Task<IActionResult> GetSeminarsByCourseId([FromRoute] long courseId, [FromQuery] bool embededGrade)
        {
            try
            {
                var seminars = await _seminarService.ListSeminarByCourseIdAsync(courseId);
                if (!embededGrade)
                {
                    return Json(seminars.Select(s => new
                    {
                        id = s.Id,
                        name = s.Name,
                        description = s.Description,
                        groupingMethod = s.IsFixed == true ? "fixed" : "random",
                        startTime = s.StartTime.ToString("YYYY-MM-dd"),
                        endTime = s.EndTime.ToString("YYYY-MM-dd")
                    }));
                }

                return Json(seminars.Select(s => new
                {
                    id = s.Id,
                    name = s.Name,
                    description = s.Description,
                    groupingMethod = s.IsFixed == true ? "fixed" : "random",
                    startTime = s.StartTime.ToString("YYYY-MM-dd"),
                    endTime = s.EndTime.ToString("YYYY-MM-dd"),
                    grade = _seminarGroupService.GetSeminarGroupByIdAsync(s.Id, User.Id()).Result.FinalGrade
                }));
            }
            catch (CourseNotFoundException)
            {
                return StatusCode(404, new {msg = "未找到课程"});
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new {msg = "课程ID格式错误"});
            }
        }

        [HttpPost("/course/{courseId:long}/seminar")]
        public async Task<IActionResult> CreateSeminarByCourseId([FromRoute] long courseId, [FromBody] Seminar newSeminar)
        {
            var userlogin = await _userService.GetUserByUserIdAsync(User.Id());
            if (userlogin.Type == Type.Teacher)
            {
                var seminarId = await _seminarService.InsertSeminarByCourseIdAsync(courseId, newSeminar);
                return Created($"/seminar/{seminarId}", new {id = seminarId});
            }

            return StatusCode(403, new {msg = "权限不足"});
        }

        /*
         * 这里用了一个foreach，但是实际用途缺不是很大。获得当前班级对应的所有讨论课信息，这个条件是基于时间的，即基于讨论课的讨论课组信息。
         * 不同的讨论课的组分数信息没有办法也不应该放在一起展示，这一点很关键。所以本Controller是直接用foreach方法List调用来完成的。
         * 已经反馈给模块组，不过也不知道改不改得了了。
         */
        [HttpGet("/course/{courseId:long}/grade")]
        public async Task<IActionResult> GetGradeByCourseId([FromRoute] long courseId)
        {
            try
            {
                var seminarGroups = await _seminarGroupService.ListSeminarGroupIdByStudentIdAsync(User.Id());
                return Json(seminarGroups.Select(s => new
                {
                    seminarName = _seminarService.GetSeminarBySeminarIdAsync(s.SeminarId).Result.Name,
                    groupName = s.Id + "组", //这里还是没有组名的问题
                    leaderName = _userService.GetUserByUserIdAsync(s.LeaderId).Result.Name,
                    presentationGrade = s.PresentationGrade,
                    reportGrade = s.ReportGrade,
                    grade = s.FinalGrade
                }));
            }
            catch (CourseNotFoundException)
            {
                return StatusCode(404, new {msg = "未找到讨论课"});
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new {msg = "课程ID格式错误"});
            }
        }
    }

    public class CourseWithProportions
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Proportions Proportions { get; set; }
    }


    public class ClassWithProportions
    {
        public string Name { get; set; }

        public string Site { get; set; }

        public string Time { get; set; }

        public Proportions Proportions { get; set; }
    }

    public class Proportions
    {
        public int Report { get; set; }
        public int Presentation { get; set; }
        public int C { get; set; }
        public int B { get; set; }
        public int A { get; set; }
    }
}