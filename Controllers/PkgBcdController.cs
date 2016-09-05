using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WMS.Models;

namespace WMS.Controllers
{
    public class PkgBcdController : SsnController
    {
        /// <summary>
        /// 箱装码管理
        /// </summary>
        public PkgBcdController()
        {
            Mdlid = "PkgBcd";
            Mdldes = "箱装码管理";
        }

        /// <summary>
        /// 根据 gdsid/bcd 查找箱装码信息 
        /// </summary>
        /// <param name="gdsid"></param>
        /// <returns></returns>
        public ActionResult GetPkgBcdByGdsid(String gdsid){
            gdsid = GetGdsidByGdsidOrBcd(gdsid);
            var qry = from e in WmsDc.wms_pkgbcd
                      where e.gdsid.Trim()==gdsid.Trim()
                      select e.pkgbcd.Trim();
            var arrqry = qry.ToArray();
            if(arrqry.Length==0){
                return RInfo("I0497");
            }

            return RSucc("成功", arrqry, "S0236");
        }

        /// <summary>
        /// 根据箱装码查找 商品信息
        /// </summary>
        /// <param name="pkgbcd"></param>
        /// <returns></returns>
        public ActionResult GetGdsByPkgBcd(String pkgbcd)
        {
            var qry = from e in WmsDc.wms_pkgbcd
                      join e1 in WmsDc.gds
                      on e.gdsid equals e1.gdsid
                      join e2 in WmsDc.emp on e.uptpsn equals e2.empid
                      where e.pkgbcd == pkgbcd.Trim()
                      select new
                      {
                          gdsid = e1.gdsid.Trim(),
                          gdsdes = e1.gdsdes.Trim(),
                          e1.bsepkg,
                          e1.spc,
                          uptpsndes = e2.empdes.Trim(),
                          uptpsn = e2.empid.Trim(),
                          e.udtdtm
                      };
            var arrqry = qry.ToArray();
            if (arrqry.Length == 0)
            {
                return RInfo("I0498");
            }

            return RSucc("成功", arrqry, "S0237");
        }

        /// <summary>
        /// 增加箱装码
        /// </summary>
        /// <param name="pkgbcd"></param>
        /// <param name="gdsid"></param>
        /// <returns></returns>
        [PWR(Pwrid = WMSConst.WMS_BACK_箱装码维护, pwrdes = "箱装码维护")]
        public ActionResult AddPkgBcd(String pkgbcd, String gdsid)
        {
            if (string.IsNullOrEmpty(pkgbcd))
            {
                return RInfo("I0500");
            }
            gdsid = GetGdsidByGdsidOrBcd(gdsid);
            if (string.IsNullOrEmpty(gdsid))
            {
                return RInfo("I0501");
            }
            var qryPkgbcd = (from e in WmsDc.wms_pkgbcd
                            join e1 in WmsDc.emp on e.uptpsn equals e1.empid
                            join e2 in WmsDc.gds on e.gdsid equals e2.gdsid
                            where e.pkgbcd == pkgbcd.Trim()
                            select new{
                                e.gdsid, e.pkgbcd, uptpsndes = e1.empdes.Trim(), e.udtdtm,
                                e.uptpsn, e2.gdsdes, e2.spc, e2.bsepkg
                            }).FirstOrDefault();
            if (qryPkgbcd!=null)
            {                
                //return RInfo("I0499", pkgbcd.Trim(), qryPkgbcd.gdsid, qryPkgbcd.gdsdes, qryPkgbcd.spc, qryPkgbcd.bsepkg, qryPkgbcd.udtdtm, qryPkgbcd.uptpsndes);
                return RInfo("I0499", qryPkgbcd);
            }
            string gdsid1 = GetGdsidByGdsidOrBcd(pkgbcd);
            if (!string.IsNullOrEmpty(gdsid1))
            {
                return RInfo("I0505");
            }
            string bcd = GetABcdByGdsid1(gdsid);
            if (bcd == pkgbcd)
            {
                return RInfo("I0504", pkgbcd, bcd);
            }

            wms_pkgbcd apkgbcd = new wms_pkgbcd();
            apkgbcd.pkgbcd = pkgbcd;
            apkgbcd.gdsid = gdsid;
            apkgbcd.uptpsn = LoginInfo.Usrid;
            apkgbcd.udtdtm = GetCurrentDate();
            WmsDc.wms_pkgbcd.InsertOnSubmit(apkgbcd);
            WmsDc.SubmitChanges();

            return RSucc("成功",qryPkgbcd,"S0238");
        }

        /// <summary>
        /// 删除箱装码
        /// </summary>
        /// <param name="pkgbcd"></param>
        /// <param name="gdsid"></param>
        /// <returns></returns>
        [PWR(Pwrid = WMSConst.WMS_BACK_箱装码维护, pwrdes = "箱装码维护")]
        public ActionResult DlPkgBcd(string pkgbcd, string gdsid)
        {
            if (string.IsNullOrEmpty(pkgbcd))
            {
                return RInfo("I0502");
            }
            gdsid = GetGdsidByGdsidOrBcd(gdsid);
            if (string.IsNullOrEmpty(gdsid))
            {
                return RInfo("I0503");
            }
            var qryPkgbcd = (from e in WmsDc.wms_pkgbcd
                             join e1 in WmsDc.emp on e.uptpsn equals e1.empid
                             where e.pkgbcd == pkgbcd.Trim() && e.gdsid == gdsid.Trim()
                             select new
                             {
                                 e.gdsid,
                                 e.pkgbcd,
                                 uptpsndes = e1.empdes.Trim(),
                                 e.uptpsn
                             }).FirstOrDefault();
            if (qryPkgbcd == null)
            {
                return RNoData("N0259");
            }

            var apkgbcd = WmsDc.wms_pkgbcd.Where(e => e.gdsid == gdsid.Trim() && e.pkgbcd == pkgbcd.Trim()).Select(e => e);
            WmsDc.wms_pkgbcd.DeleteAllOnSubmit(apkgbcd);
            WmsDc.SubmitChanges();

            return RSucc("成功", null, "S0239");
        }
    }
}
