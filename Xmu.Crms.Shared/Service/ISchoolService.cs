using System.Collections.Generic;
using System.Threading.Tasks;
using Xmu.Crms.Shared.Models;

namespace Xmu.Crms.Shared.Service
{
    /// <summary>
    ///     @author LiuAiqi
    ///     @version 2.00
    /// </summary>
    public interface ISchoolService : Orleans.IGrainWithIntegerKey
    {
        /// <summary>
        ///     按城市名称查学校.
        ///     @author LiuAiqi
        /// </summary>
        /// <param name="city">城市名称</param>
        /// <returns>list 学校列表</returns>
        Task<IList<School>> ListSchoolByCityAsync(string city);

        /// <summary>
        ///     添加学校.
        ///     @author LiuAiqi
        /// </summary>
        /// <param name="school">学校的信息</param>
        /// <returns>schoolId 学校的id</returns>
        Task<long> InsertSchoolAsync(School school);

        /// <summary>
        ///     获取省份列表.
        ///     @author LiuAiqi
        /// </summary>
        /// <returns>list 省份名称列表</returns>
        Task<IList<string>> ListProvinceAsync();

        /// <summary>
        ///     获取城市列表.
        ///     @author LiuAiqi
        /// </summary>
        /// <param name="province">省份名称</param>
        /// <returns>list 城市名称列表</returns>
        Task<IList<string>> ListCityAsync(string province);

        /// <summary>
        ///     获取学校信息.
        ///     @author LiuAiqi
        /// </summary>
        /// <param name="schoolId">学校id</param>
        /// <returns>SchoolBO 学校信息</returns>
        Task<School> GetSchoolBySchoolIdAsync(long schoolId);
    }
}