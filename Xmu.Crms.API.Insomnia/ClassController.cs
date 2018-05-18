using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xmu.Crms.Shared.Exceptions;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;
using Type = Xmu.Crms.Shared.Models.Type;

namespace Xmu.Crms.Insomnia
{
    [Route("")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ClassController : Controller
    {
        private readonly IClassService _classService;
        private readonly ICourseService _courseService;
        private readonly CrmsContext _db;
        private readonly IFixGroupService _fixGroupService;
        private readonly ISeminarService _seminarService;
        private readonly IUserService _userService;

        public ClassController(CrmsContext db, ICourseService courseService, IClassService classService,
            IUserService userService, IFixGroupService fixGroupService,
            ISeminarService seminarService)
        {
            _db = db;
            _courseService = courseService;
            _classService = classService;
            _userService = userService;
            _fixGroupService = fixGroupService;
            _seminarService = seminarService;
        }

        [HttpGet("/class")]
        public async Task<IActionResult> GetUserClasses([FromQuery] string courseName, [FromQuery] string courseTeacher)
        {
            //List<ClassInfo> classes = new List<ClassInfo>();
            try
            {
                IList<ClassInfo> classes;
                if (string.IsNullOrEmpty(courseName) && string.IsNullOrEmpty(courseTeacher))
                {
                    classes = await _classService.ListClassByUserIdAsync(User.Id());
                }
                else if (string.IsNullOrEmpty(courseTeacher))
                {
                    classes = await _courseService.ListClassByCourseNameAsync(courseName);
                }
                else if (string.IsNullOrEmpty(courseName))
                {
                    classes = await _courseService.ListClassByTeacherNameAsync(courseTeacher);
                }
                else
                {
                    var c = (await _courseService.ListClassByCourseNameAsync(courseName)).ToHashSet();
                    c.IntersectWith(await _courseService.ListClassByTeacherNameAsync(courseTeacher));
                    classes = c.ToList();
                }

                return Json(await Task.WhenAll(classes.Select(async c =>
                {
                    var co = await _courseService.GetCourseByCourseIdAsync(c.CourseId);
                    return new
                    {
                        id = c.Id,
                        name = c.Name,
                        site = c.Site,
                        time = c.ClassTime,
                        courseId = c.CourseId,
                        courseName = co.Name,
                        courseTeacher =
                            (await _userService
                                .GetUserByUserIdAsync(co.TeacherId))
                                .Name,
                        numStudent = _db.Entry(c).Collection(cl => cl.CourseSelections).Query().Count()
                    };
                })));
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new {msg = "用户ID输入格式错误"});
            }
        }

        [HttpGet("/class/{classId:long}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetClassById([FromRoute] long classId)
        {
            try
            {
                var cls = await _classService.GetClassByClassIdAsync(classId);
                var sems = (await _seminarService.ListSeminarByCourseIdAsync(cls.CourseId)).FirstOrDefault(s =>
                    (_classService.GetCallStatusByIdAsync(s.Id, cls.Id).Result?.Status ?? 0) == 1);
                return Json(new
                {
                    id = cls.Id,
                    name = cls.Name,
                    time = cls.ClassTime,
                    site = cls.Site,
                    courseId = cls.CourseId,
                    calling = sems?.Id ?? -1,
                    proportions = new
                    {
                        report = cls.ReportPercentage,
                        presentation = cls.PresentationPercentage,
                        c = cls.ThreePointPercentage,
                        b = cls.FourPointPercentage,
                        a = cls.FivePointPercentage
                    }
                });
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new {msg = "班级ID输入格式有误"});
            }
        }

        [HttpDelete("/class/{classId:long}")]
        public async Task<IActionResult> DeleteClassById([FromRoute] long classId)
        {
            try
            {
                var userlogin = await _userService.GetUserByUserIdAsync(User.Id());
                if (userlogin.Type == Type.Teacher)
                {
                    await _classService.DeleteClassByClassIdAsync(classId);
                    return NoContent();
                }

                return StatusCode(403, new {msg = "权限不足"});
            }
            catch (ClassNotFoundException)
            {
                return StatusCode(404, new {msg = "班级不存在"});
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new {msg = "班级ID输入格式有误"});
            }
        }

        [HttpPut("/class/{classId:long}")]
        public async Task<IActionResult> UpdateClassById([FromRoute] long classId, [FromBody] ClassWithProportions updated)
        {
            try
            {
                var userlogin = await _userService.GetUserByUserIdAsync(User.Id());
                if (userlogin.Type != Type.Teacher)
                {
                    return StatusCode(403, new {msg = "权限不足"});
                }

                await _classService.UpdateClassByClassIdAsync(classId, new ClassInfo
                {
                    Id = classId,
                    Name = updated.Name,
                    ClassTime = updated.Time,
                    Site = updated.Site,
                    ThreePointPercentage = updated.Proportions.C,
                    FourPointPercentage = updated.Proportions.B,
                    FivePointPercentage = updated.Proportions.A,
                    ReportPercentage = updated.Proportions.Report,
                    PresentationPercentage = updated.Proportions.Presentation
                });
                return NoContent();
            }
            catch (ClassNotFoundException)
            {
                return StatusCode(404, new {msg = "班级不存在"});
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new {msg = "班级ID输入格式有误"});
            }
        }

        [HttpGet("/class/{classId:long}/student")]
        public async Task<IActionResult> GetStudentsByClassId([FromRoute] long classId, [FromQuery] string numBeginWith,
            string nameBeginWith)
        {
            try
            {
                var users = await _userService.ListUserByClassIdAsync(classId, numBeginWith, nameBeginWith);
                return Json(users.Select(u => new {id = u.Id, name = u.Name, number = u.Number}));
            }
            catch (ClassNotFoundException)
            {
                return StatusCode(404, new {msg = "班级格式输入有误"});
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new {msg = "班级ID输入格式有误"});
            }

            //return Json(new List<ClassInfo>());
        }

        [HttpPost("/class/{classId:long}/student")]
        public async Task<IActionResult> SelectClass([FromRoute] long classId, [FromBody] UserInfo student)
        {
            try
            {
                var userlogin = await _userService.GetUserByUserIdAsync(User.Id());
                if (userlogin.Type == Type.Student)
                {
                    if (User.Id() == student.Id)
                    {
                        await _classService.InsertCourseSelectionByIdAsync(student.Id, classId);
                        return Created($"/class/{classId}/student/{student.Id}",
                            new Dictionary<string, string> {["url"] = $"/class/{classId}/student/{student.Id}"});
                    }

                    return StatusCode(403, new {msg = "学生无法为他人选课"});
                }

                return StatusCode(403, new {msg = "权限不足"});
            }
            catch (ClassNotFoundException)
            {
                return StatusCode(404, new {msg = "班级不存在"});
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new {msg = "班级ID输入格式有误"});
            }
        }

        [HttpDelete("/class/{classId:long}/student/{studentId:long}")]
        public async Task<IActionResult> DeselectClass([FromRoute] long classId, [FromRoute] long studentId)
        {
            try
            {
                var userlogin = await _userService.GetUserByUserIdAsync(User.Id());
                if (userlogin.Type == Type.Student)
                {
                    if (studentId == User.Id())
                    {
                        await _classService.DeleteCourseSelectionByIdAsync(studentId, classId);
                        return NoContent();
                    }

                    return StatusCode(403, new {msg = "用户无法为他人退课"});
                }

                return StatusCode(403, new {msg = "权限不足"});
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new {msg = "错误的ID格式"});
            }
        }

        [HttpGet("/class/{classId}/classgroup")]
        public async Task<IActionResult> GetUserClassGroupByClassId([FromRoute] long classId)
        {
            try
            {
                var userlogin = await _userService.GetUserByUserIdAsync(User.Id());
                if (userlogin.Type != Type.Student)
                {
                    return StatusCode(403, new {msg = "权限不足"});
                }

                var fixGroup = await _fixGroupService.GetFixedGroupByIdAsync(User.Id(), classId);
                var leader = fixGroup.Leader ?? await _userService.GetUserByUserIdAsync(fixGroup.LeaderId);
                var members = await _fixGroupService.ListFixGroupMemberByGroupIdAsync(fixGroup.Id);
                var result = Json(
                    new
                    {
                        leader = new
                        {
                            id = leader.Id,
                            name = leader.Name,
                            number = leader.Number
                        },
                        members = members.Select(m => new
                        {
                            id = m.Id,
                            name = m.Name,
                            number = m.Number
                        })
                    });
                return result;
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new {msg = "课程ID格式错误"});
            }
        }

        [HttpPut("/class/{classId}/classgroup/resign")]
        public IActionResult GroupLeaderResignByClassId([FromRoute] long classId, [FromBody] UserInfo student)
        {
            try
            {
                //var groupId = _fixGroupService.GetFixedGroupByIdAsync()
                //_seminarGroupService.ResignLeaderByIdAsync
                return NoContent();
            }
            catch (ClassNotFoundException)
            {
                return StatusCode(404, new {msg = "不存在当前班级"});
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new {msg = "班级ID格式错误"});
            }
        }

        [HttpPut("/class/{classId}/classgroup/assign")]
        public IActionResult GroupLeaderAssignByClassId([FromRoute] long classId, [FromBody] UserInfo student)
        {
            try
            {
                //var groupId = _fixGroupService.GetFixedGroupByIdAsync()
                //_seminarGroupService.ResignLeaderByIdAsync
                return NoContent();
            }
            catch (ClassNotFoundException)
            {
                return StatusCode(404, new {msg = "不存在当前班级"});
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new {msg = "班级ID格式错误"});
            }
        }

        [HttpPut("/class/{classId}/classgroup/add")]
        public async Task<IActionResult> AddGroupMemberByClassId([FromRoute] long classId, [FromBody] UserInfo student)
        {
            try
            {
                var group = await _fixGroupService.GetFixedGroupByIdAsync(User.Id(), classId);
                await _fixGroupService.InsertStudentIntoGroupAsync(student.Id, @group.Id);
                return NoContent();
            }
            catch (ClassNotFoundException)
            {
                return StatusCode(404, new {msg = "不存在当前班级"});
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new {msg = "班级ID格式错误"});
            }
        }

        [HttpPut("/class/{classId}/classgroup/remove")]
        public async Task<IActionResult> RemoveGroupMemberByClassId([FromRoute] long classId, [FromBody] UserInfo student)
        {
            try
            {
                var group = await _fixGroupService.GetFixedGroupByIdAsync(User.Id(), classId);
                await _fixGroupService.DeleteFixGroupUserByIdAsync(@group.Id, student.Id);
                return NoContent();
            }
            catch (ClassNotFoundException)
            {
                return StatusCode(404, new {msg = "不存在当前班级"});
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new {msg = "班级ID格式错误"});
            }
        }
    }
}