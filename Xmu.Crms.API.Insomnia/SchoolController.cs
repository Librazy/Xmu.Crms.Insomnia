using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;

namespace Xmu.Crms.Insomnia
{
    [Route("")]
    [Produces("application/json")]
    public class SchoolController : Controller
    {
        private readonly ISchoolService _schoolService;

        public SchoolController(ISchoolService schoolService) => _schoolService = schoolService;

        [HttpGet("/school")]
        public async Task<IActionResult> GetSchools([FromQuery] string city)
        {
            var schools = await _schoolService.ListSchoolByCityAsync(city);
            return Json(schools.Select(t => new
            {
                id = t.Id,
                name = t.Name,
                province = t.Province,
                city = t.City
            }));
        }

        [HttpGet("/school/{schoolId:long}")]
        public async Task<IActionResult> GetSchoolById([FromRoute] long schoolId)
        {
            try
            {
                var schoolinfo = await _schoolService.GetSchoolBySchoolIdAsync(schoolId);
                return Json(new {name = schoolinfo.Name, province = schoolinfo.Province, city = schoolinfo.City});
            }
            catch (ArgumentException)
            {
                return StatusCode(400, new {msg = "学校ID输入格式有误"});
            }
        }


        /*
         * 这里school的查找有问题
         */
        [HttpPost("/school")]
        public async Task<IActionResult> CreateSchool([FromBody] School newSchool)
        {
            var schoolId = await _schoolService.InsertSchoolAsync(newSchool);
            return Created("/school/" + schoolId, newSchool);
        }
    }
}