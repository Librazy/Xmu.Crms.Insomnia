using System;
using System.Collections.Generic;
using System.Linq;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;

namespace Xmu.Crms.Services.HighGrade
{
    public class SchoolService : ISchoolService
    {
        private readonly CrmsContext _db;

        public SchoolService(CrmsContext db) => _db = db;

        public School GetSchoolBySchoolId(long schoolId)
        {
            var school = _db.School.SingleOrDefault(s => s.Id == schoolId);

            if (school == null)
            {
                throw new ArgumentOutOfRangeException();
            }

            return school;
        }

        public long InsertSchool(School school)
        {
            var s = new School
            {
                Id = school.Id,
                Name = school.Name,
                Province = school.Province,
                City = school.City
            };

            var sch = _db.School.Where(sc => sc.Name == s.Name).ToList();
            if (sch == null)
            {
                throw new Exception();
            }

            _db.School.Add(s);
            _db.SaveChanges();
            return s.Id;
        }

        public IList<string> ListCity(string province)
        {
            var cities = _db.School.Where(s => s.Province == province).Select(s => s.City).Distinct().ToList();

            if (cities == null)
            {
                throw new Exception();
            }

            return cities;
        }

        public IList<string> ListProvince()
        {
            var provinces = _db.School.Select(s => s.Province).Distinct().ToList();

            if (provinces == null)
            {
                throw new Exception();
            }

            return provinces;
        }

        public IList<School> ListSchoolByCity(string city)
        {
            var schools = _db.School.Where(s => s.City == city).ToList();

            if (schools == null)
            {
                throw new Exception();
            }

            return schools;
        }
    }
}