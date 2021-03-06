﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WMS;
using WMS.Models;
using WMS.Common;
using System.Web.Routing;
using System.Data.SqlClient;
using System.Text;
using System.Transactions;


namespace WMS.Controllers
{
    /// <summary>
    /// 收货单模块（整单收货）
    /// </summary>
    public class RecievController : SsnController
    {        
           


        /// <summary>
        /// 根据供应商提取采购订单
        /// </summary>
        /// <param name="prvid">供应商编码</param>
        /// <returns>ResultMessage对象</returns>
        [PWR(Pwrid = WMSConst.WMS_BACK_收货查询, pwrdes = "收货查询")]
        public ActionResult GetPurchaseByPrv(String prvid)
        {            
            var qry = from e in WmsDc.odr
                      join e1 in WmsDc.prv on e.prvid equals e1.prvid
                      join e2 in WmsDc.dpt on e.dptid  equals e2.dptid
                      where e.bllid == WMSConst.BLL_TYPE_PURCHASE
                      && e.prvid == prvid
                      && e.savdptid == LoginInfo.DefSavdptid                      
                      && e.ordstu == (char)ORD_STATUS.AUDIT && e.zdflg == GetN()
                      && dpts.Contains(e.dptid.Trim())
                      //&& e.istel == GetN()
                      select new
                      {
                          e.arvdat,
                          e.bllid,
                          e.odrno,
                          e.prvid,
                          e.savdptid,
                          e.ordstu,
                          e.zdflg,
                          e.istel,
                          e1.prvdes,
                          e2.dptdes
                      };
            var arrqry = qry.ToArray();

            //1.未找到供应商采购订单
            if (arrqry.Count() <= 0)
            {                
                return RInfo( "I0224" );
            }


            //2.返回供应商采购订单            
            return RSucc("成功", arrqry, "S0108");            
        }

        /// <summary>
        /// 得到仓储单查询
        /// </summary>
        /// <param name="wmsno">单据号</param>
        /// <returns></returns>
        protected IQueryable<object> GetWmsbll(String wmsno)
        {
            var qry = from e in WmsDc.wms_bllmst
                      where e.wmsno == wmsno
                      && e.bllid == WMSConst.BLL_TYPE_REVIECEBLL
                      select new
                      {
                          dtl = (from e1 in WmsDc.wms_blldtl
                                 join e2 in WmsDc.gds on e1.gdsid equals e2.gdsid
                                 join e3 in WmsDc.v_wms_pkg on new { e2.gdsid } equals new { e3.gdsid }
                                 into joinPkg from e4 in joinPkg.DefaultIfEmpty()
                                 where e1.wmsno == wmsno
                                 && e1.bllid == WMSConst.BLL_TYPE_REVIECEBLL
                                 select new
                                 {
                                     e1.barcode,
                                     e1.bcd,
                                     e1.bkr,
                                     e1.bllid,
                                     e1.bokdat,
                                     e1.bokflg,
                                     e1.brief,
                                     e1.bthno,
                                     e1.gdsid,
                                     e1.gdstype,
                                     e1.pkgid,
                                     e1.preqty,
                                     e1.prvid,
                                     e1.qty,
                                     e1.rcdidx,
                                     e1.vlddat,
                                     e1.wmsno,
                                     e2.spc,
                                     e2.gdsdes,
                                     e2.bsepkg,
                                     pkg03 = GetPkgStr(e1.qty, e4.cnvrto,e4.pkgdes),
                                     pkg03pre = GetPkgStr(e1.preqty, e4.cnvrto, e4.pkgdes)
                                 }).ToArray()
                      };
            return qry;
        }

        //public ActionResult 
        /// <summary>
        /// 得到会计期间未审核的单据
        /// </summary>
        /// <param name="fscprdid"></param>
        /// <returns></returns>
        public ActionResult FindUnRcvBllsByFscprdid(String fscprdid)
        {
            var qry = from e in WmsDc.wms_bllmst
                      join e5 in WmsDc.odr on e.lnknewno equals e5.odrno
                      join e6 in WmsDc.dpt on e5.dptid equals e6.dptid
                      join e7 in WmsDc.prv on e.prvid equals e7.prvid
                      join e8 in WmsDc.emp on e.mkr equals e8.empid
                      where e.mkedat.Substring(2, 4) == fscprdid
                      && e.chkflg == GetN()
                      && e.savdptid == LoginInfo.DefSavdptid
                      && (spqus.Contains(e.qu) || dtqus.Contains(e.qu))
                      && e.bllid == WMSConst.BLL_TYPE_REVIECEBLL
                      //&& e.mkr == LoginInfo.Usrid
                      select new
                      {
                          e.arvdat,
                          e.bllid,
                          e.brief,
                          e.chkdat,
                          e.chkflg,
                          e.ckr,
                          e.hndbllno,
                          e.huojia,
                          e.lnknewbllid,
                          e.lnknewbrief,
                          e.lnknewno,
                          e.mkedat,
                          e.mkr,
                          mkrdes = e8.empdes,
                          e.odrdat,
                          e.opr,
                          e.prvid,
                          e.qu,
                          e.savdptid,
                          e.tongdao,
                          e.wmsno,
                          e7.prvdes
                      };

            return RSucc("成功", qry.ToArray(), "S0109");
        }

        /// <summary>
        /// 查找当天的收货单
        /// </summary>
        /// <param name="day">查询日期</param>
        /// <returns></returns>
        [PWR(Pwrid = WMSConst.WMS_BACK_收货查询, pwrdes = "收货查询")]
        public ActionResult FindCurrDayRcvBlls(String day)
        {

            var qry = from e in WmsDc.wms_bllmst
                      join e5 in WmsDc.odr on e.lnknewno equals e5.odrno
                      join e6 in WmsDc.dpt on e5.dptid equals e6.dptid
                      join e7 in WmsDc.prv on e.prvid equals e7.prvid
                      join e8 in WmsDc.emp on e.mkr equals e8.empid
                      where e.mkedat.Substring(0, 8) == day
                      && e.savdptid == LoginInfo.DefSavdptid
                      && e.bllid == WMSConst.BLL_TYPE_REVIECEBLL
                      //&& e.mkr == LoginInfo.Usrid
                      select new
                      {
                          e.arvdat,
                          e.bllid,
                          e.brief,
                          e.chkdat,
                          e.chkflg,
                          e.ckr,
                          e.hndbllno,
                          e.huojia,
                          e.lnknewbllid,
                          e.lnknewbrief,
                          e.lnknewno,
                          e.mkedat,
                          e.mkr,
                          mkrdes = e8.empdes,
                          e.odrdat,
                          e.opr,
                          e.prvid,
                          e.qu,
                          e.savdptid,
                          e.tongdao,
                          e.wmsno,
                          e7.prvdes
                      };
                      /*select new Wmsbll
                      {
                          mst = e,
                          dptdes = e6.dptdes,
                          prv = (from e4 in WmsDc.prv
                                 where e4.prvid == e.prvid
                                 select new WmsBllPrv
                                 {
                                     prvid = e4.prvid.Trim(),
                                     prvdes = e4.prvdes.Trim(),
                                 }).Single(),
                          dtl = (from e1 in WmsDc.wms_blldtl
                                 where e1.wmsno == e.wmsno
                                 select e1).ToArray(),
                          gds = (from e2 in WmsDc.wms_blldtl
                                 join e3 in WmsDc.gds on e2.gdsid equals e3.gdsid
                                 where e2.wmsno == e.wmsno
                                 select new WmsBllGds
                                 {
                                     gdsid = e3.gdsid.Trim(),
                                     gdsdes = e3.gdsdes.Trim(),
                                     bsepkg = e3.bsepkg.Trim(),
                                     spc = e3.spc.Trim()
                                 }).ToArray(),
                          dtls = (from e1 in WmsDc.wms_blldtl
                                  join e2 in WmsDc.gds on e1.gdsid equals e2.gdsid
                                  where e1.wmsno == e.wmsno
                                  select new
                                  {
                                      e1.wmsno,
                                      e1.vlddat,
                                      e1.rcdidx,
                                      e1.qty,
                                      e1.prvid,
                                      e1.preqty,
                                      e1.pkgid,
                                      e1.gdstype,
                                      e1.gdsid,
                                      e1.brief,
                                      e1.bokflg,
                                      e1.bokdat,
                                      e1.bllid,
                                      e1.bkr,
                                      e1.bcd,
                                      e1.barcode,
                                      e2.gdsdes,
                                      e2.spc,
                                      e2.bsepkg
                                  }
                                       ).ToArray()*/

            
            return RSucc("成功", qry.ToArray(), "S0110");
        }

        /// <summary>
        /// 得到收货单
        /// </summary>
        /// <param name="wmsno"></param>
        /// <returns></returns>
        [PWR(Pwrid = WMSConst.WMS_BACK_收货查询, pwrdes = "收货查询")]
        public ActionResult GetRecievBll(String wmsno)
        {
            var qry = GetWmsbll(wmsno);
            
            var arrqry = qry.ToArray();
            if (arrqry.Length <= 0)
            {
                return RNoData("N0124");
            }
            
            return RSucc("成功", arrqry[0], "S0111");
        }

        /// <summary>
        /// 生成收货单
        /// </summary>
        /// <param name="odrnos">订单编号</param>
        /// <returns></returns>
        [PWR(Pwrid = WMSConst.WMS_BACK_收货制单, pwrdes = "收货制单")]
        public ActionResult GenerateRecievBlls(String odrnos)
        {
            String[] odrno = odrnos.Split(',');
            JsonResult jr = null;

            List<object> lstobj = new List<object>();
            foreach (String odr in odrno)
            {
                jr = (JsonResult)GenerateRecievBll(odr.Trim());
                //WmsDc.SubmitChanges();

                Rm = (ResultMessage)jr.Data;
                
                if (Rm.ResultCode != ResultMessage.RESULTMESSAGE_SUCCESS)
                {
                    return jr;
                }

                lstobj.Add(Rm.ResultObject);
            }
            Rm.ResultObject = lstobj;

            return Json(Rm, "application/json", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 查到未审核单据
        /// </summary>
        /// <returns></returns>
        [PWR(Pwrid = WMSConst.WMS_BACK_收货查询, pwrdes = "收货查询")]
        public ActionResult FindUnAdtRecievBll(char chkflg, String prvid, String bdtm, String edtm)
        {
            
            var qry = from e in WmsDc.wms_bllmst                      
                      where qus.Contains(e.qu.Trim())
                      && e.savdptid == LoginInfo.DefSavdptid
                      && e.chkflg == GetN()                      
                      && e.bllid == WMSConst.BLL_TYPE_REVIECEBLL
                      select e;
            if( chkflg!=null ){
                qry = qry.Where(w=>w.chkflg==chkflg).Select(e=>e);
            }

            var qry1 = from e in qry
                       join e1 in WmsDc.prv on e.prvid equals e1.prvid
                       select new Wmsbll
                       {
                           mst = e,
                           prv = new WmsBllPrv
                           {
                               prvid = e1.prvid,
                               prvdes = e1.prvdes
                           }
                       };

            var bllmsts = qry.ToArray();
            if (bllmsts.Length <= 0)
            {
                return RNoData("N0125");
            }

            return RSucc("成功", bllmsts, "S0112");
        }

        /// <summary>
        /// 生成收货单
        /// </summary>
        /// <param name="odrno">订单编号</param>
        /// <returns></returns>
        [PWR(Pwrid = WMSConst.WMS_BACK_收货制单, pwrdes = "收货制单")]
        public ActionResult GenerateRecievBll(String odrno)
        {
            String qu = GetQuByDptid(odrno, LoginInfo.DefStoreid);
            return MakeNewBllNo(
                LoginInfo.DefSavdptid,
                qu,
                WMSConst.BLL_TYPE_REVIECEBLL,
                ((bllno) =>
                {                                        
                    //WmsDc.Transaction = WmsDc.Connection.BeginTransaction();                    
                    var qry = from e in WmsDc.odr
                              where e.odrno == odrno
                              && e.savdptid == LoginInfo.DefSavdptid
                              && e.bllid == WMSConst.BLL_TYPE_PURCHASE
                              && e.ordstu == (char)ORD_STATUS.AUDIT && e.zdflg == GetN()
                              select new
                              {
                                  mst = e,
                                  dtl = from e1 in WmsDc.odrdtl
                                        where e1.odrno == odrno
                                        select e1
                              };
                    var arrqry = qry.ToArray();
                    //1.未找到供应商采购订单
                    if (arrqry.Count() <= 0)
                    {
                        return RRInfo("I0443");
                        
                    }

                    //2.查看采购单据是否已经转过为收货单
                    var qryshd = from e in WmsDc.wms_bllmst
                              where e.bllid == WMSConst.BLL_TYPE_REVIECEBLL
                              && e.lnknewno == odrno
                              select e;
                    if (qryshd.Count() > 0)
                    {
                        return RRInfo("I0444");

                    }
                    
                    var odr = arrqry[0];
                    //修改转单标记
                    String sNow = DateTime.Now.ToString("yyyyMMdd");
                    String cmd = "update odr set zdflg='" + GetY() + "',zddat={0} where odrno={1}";
                    WmsDc.ExecuteCommand(cmd, new[] { sNow, odrno });



                    //===================== 生成一张新的收货单 =====================
                    //--------------------- 1.生成主单 -----------------------------
                    wms_bllmst bllmst = new wms_bllmst();
                    bllmst.wmsno = bllno;                    
                    bllmst.hndbllno = odr.mst.hndbllno;
                    bllmst.bllid = WMSConst.BLL_TYPE_REVIECEBLL;
                    bllmst.prvid = odr.mst.prvid;
                    bllmst.savdptid = odr.mst.savdptid;
                    //得到登录仓库所在部门的区位码
                    GetRealteQuResult realte = GetRealteQu(odr.mst.dptid, LoginInfo.DefSavdptid);
                    bllmst.qu = realte.qu;
                    ////正在生成拣货单，请稍候重试
                    //string quRetrv = bllmst.qu;
                    //if (DoingRetrieve(LoginInfo.DefStoreid, quRetrv))
                    //{
                    //    return RRInfo( "I0225" );
                    //}

                    /*bllmst.tongdao = odr.mst.tongdao;
                    bllmst.huojia = odr.mst.huojia;*/
                    bllmst.odrdat = odr.mst.odrdat;
                    bllmst.arvdat = odr.mst.arvdat;
                    bllmst.mkr = LoginInfo.Usrid;
                    bllmst.mkedat = DateTime.Now.ToString("yyyyMMddhhmmss");
                    bllmst.ckr = "";
                    bllmst.chkflg = GetN();
                    bllmst.chkdat = "";
                    bllmst.opr = LoginInfo.Usrid;
                    bllmst.brief = odr.mst.brief;
                    bllmst.lnknewbllid = odr.mst.bllid;
                    bllmst.lnknewno = odr.mst.odrno;
                    bllmst.lnknewbrief = odr.mst.brief;
                    this.WmsDc.wms_bllmst.InsertOnSubmit(bllmst);
                    //--------------------- 2.生成明细 -----------------------------
                    List<wms_blldtl> blldtls = new List<wms_blldtl>();
                    List<WmsBllGds> gdss = new List<WmsBllGds>();
                    foreach (odrdtl dtl in odr.dtl)
                    {
                        if (dtl.qty != 0)
                        {
                            wms_blldtl blldtl = new wms_blldtl();
                            blldtl.wmsno = bllmst.wmsno;
                            blldtl.bllid = bllmst.bllid;
                            blldtl.rcdidx = dtl.rcdidx;
                            blldtl.barcode = "";
                            blldtl.gdsid = dtl.gdsid;
                            blldtl.pkgid = "01";   //dtl.pkgid;
                            blldtl.qty = 0;
                            blldtl.preqty = Math.Round(dtl.qty, 4, MidpointRounding.AwayFromZero);
                            blldtl.gdstype = WMSConst.GDS_TYPE_NORMAL;
                            blldtl.bthno = dtl.bthno == null ? "" : dtl.bthno;
                            blldtl.vlddat = dtl.vlddat == null ? "" : dtl.vlddat;
                            blldtl.bcd = dtl.bcd == null ? "" : dtl.bcd;
                            blldtl.prvid = bllmst.prvid == null ? "" : dtl.prvid;
                            blldtl.bkr = "";
                            blldtl.bokflg = GetN();
                            blldtl.bokdat = "";
                            blldtl.brief = "";

                            WmsBllGds gds = WmsDc.gds
                                      .Where(e => e.gdsid == dtl.gdsid)
                                      .Select(e => new WmsBllGds
                                      {
                                          gdsid = e.gdsid.Trim(),
                                          gdsdes = e.gdsdes.Trim(),
                                          spc = e.spc.Trim(),
                                          bsepkg = e.bsepkg.Trim()
                                      }).Single();
                            gdss.Add(gds);

                            blldtls.Add(blldtl);
                        }
                    }
                    this.WmsDc.wms_blldtl.InsertAllOnSubmit(blldtls);
                    //===================== 生成一张新的收货单 =====================

                    try
                    {
                      var dtlstmp =  (from e in blldtls
                                    join e1 in gdss on e.gdsid.Trim() equals e1.gdsid.Trim()
                                    select new
                                    {
                                        e.wmsno,
                                        e.vlddat,
                                        e.rcdidx,
                                        e.qty,
                                        e.prvid,
                                        e.preqty,
                                        e.pkgid,
                                        e.gdstype,
                                        e.gdsid,
                                        e.brief,
                                        e.bokflg,
                                        e.bokdat,
                                        e.bllid,
                                        e.bkr,
                                        e.bcd,
                                        e.barcode,
                                        e1.gdsdes,
                                        e1.spc,
                                        e1.bsepkg
                                    }
                                         ).ToArray();
                        Wmsbll obj = new Wmsbll
                        {
                            mst = bllmst,
                            dptdes = (from e in WmsDc.dpt
                                      where e.dptid == odr.mst.dptid
                                      select e.dptdes).Single(),
                            prv = (from e4 in WmsDc.prv
                                   where e4.prvid == bllmst.prvid
                                   select new WmsBllPrv
                                   {
                                       prvid = e4.prvid.Trim(),
                                       prvdes = e4.prvdes.Trim(),
                                   }).Single(),                            
                            dtl = blldtls.ToArray(),
                            gds = gdss.ToArray(),
                            dtls = dtlstmp
                        };

                        //obj = ((Wmsbll)Rm.ResultObject);
                        var objrm = new
                        {
                            obj.mst.arvdat,
                            obj.mst.bllid,
                            obj.mst.brief,
                            obj.mst.chkdat,
                            obj.mst.chkflg,
                            obj.mst.ckr,
                            obj.mst.hndbllno,
                            obj.mst.huojia,
                            obj.mst.lnknewbllid,
                            obj.mst.lnknewbrief,
                            obj.mst.lnknewno,
                            obj.mst.mkedat,
                            obj.mst.mkr,
                            obj.mst.odrdat,
                            obj.mst.opr,
                            obj.mst.prvid,
                            obj.mst.qu,
                            obj.mst.savdptid,
                            obj.mst.tongdao,
                            obj.mst.wmsno,
                            obj.prv.prvdes,
                            obj.dptdes
                        };
                        Rm.ResultObject = objrm;
                                           
                        return Rm;
                    }
                    catch (Exception ex)
                    {
                        return RRErr(ex.Message, "E0065");

                    }

                }));
                    
        }

        /// <summary>
        /// 推荐仓位
        /// </summary>
        /// <returns></returns>        
        protected String SuggestBarcode(String qu, Action<wms_cangwei> action)
        {
            String barcode = null;
            wms_cangwei cw = null;
            /*String[] barcodes = GetEmptyBarcodeByQu(qu);
            var qrybar = from e in WmsDc.wms_cangwei
                         where e.tjflg == GetY() && e.isvld == GetY() && e.tpflg == GetN()
                         && qu == e.qu && barcodes.Contains(e.barcode)                         
                         orderby e.barcode
                         select e;*/
            var qrybar = from e in WmsDc.wms_cangwei
                         where e.savdptid == LoginInfo.DefSavdptid
                         && e.qu == qu
                         && e.isvld == GetY() && e.tjflg == GetY()
                         && e.tpflg == GetN() && e.kcflg == WMSConst.KC_FLG_NONQTY
                         orderby new { e.ceng, e.barcode }
                         select e;

            var arrqrybar = qrybar.Take(1).ToArray();
            if (arrqrybar.Length > 0)
            {
                cw = arrqrybar[0];
                barcode = cw.barcode;
                action(cw);
            }
            else
            {
                barcode = null;
            }
            
            return barcode;
        }

        /// <summary>
        /// 登帐收货单
        /// </summary>
        /// <param name="wmsno">收货单号</param>
        /// <returns></returns>
        [PWR(Pwrid = WMSConst.WMS_BACK_收货审核, pwrdes = "收货审核")]
        public ActionResult BokReciev(String wmsno)
        {
            var qry = from e in WmsDc.wms_bllmst
                      where e.bllid == WMSConst.BLL_TYPE_REVIECEBLL
                      && e.wmsno == wmsno
                      select e;
            var arrqry = qry.ToArray();
            if (arrqry.Length <= 0)
            {
                return RNoData("N0239");

            }
            wms_bllmst bllmst = arrqry[0];

            return MakeNewBllNo(
                LoginInfo.DefSavdptid,
                bllmst.qu,
                WMSConst.BLL_TYPE_UPBLL, (bllno) =>
            {
                
                ////正在生成拣货单，请稍候重试
                //string quRetrv = bllmst.qu;
                //if (DoingRetrieve(LoginInfo.DefStoreid, quRetrv))
                //{
                //    return RRInfo( "I0226" );
                //}

                //审核人是否是制单人
                /*if (IsSameLogin(bllmst.mkr))
                {
                    return RRInfo("I0445" ,LoginInfo.Usrid  ,bllmst.mkr );

                }*/

                #region 判断收货单是否已经登帐
                if (bllmst.chkflg == GetY())
                {
                    return RRInfo("I0446" ,wmsno);

                }
                #endregion

                #region 判断操作员是否有审核该单据的权限
                //0.判断操作员是否有审核该单据的权限
                var dtapwrs = from e in LoginInfo.DatPwrs
                              select e.qu;
                if (!dtapwrs.Contains(bllmst.qu.Trim()))
                {
                    return RRInfo("I0447");

                }
                #endregion
                
                #region 是否该单据下的所有商品都已经审核
                //1.是否该单据下的所有商品都已经审核            
                var qry1 = from e in WmsDc.wms_blldtl
                           where e.bllid == WMSConst.BLL_TYPE_REVIECEBLL
                           && e.wmsno == wmsno
                           select e;
                var arrqry1 = qry1.ToArray();
                if (arrqry1.Length <= 0)
                {
                    return RRNoData("N0240");

                }
                foreach (wms_blldtl dtl in arrqry1)
                {
                    if (dtl.bokflg == GetN())
                    {
                        return RRInfo("I0448" ,dtl.gdsid );

                    }
                }
                #endregion
                
                #region 修改采购单实收数量
                //判断是否有比应收数量更大的商品
                var qrytpdtl = from e in WmsDc.wms_blltp
                               where e.wmsno == wmsno && e.bllid == WMSConst.BLL_TYPE_REVIECEBLL
                               group e by new { e.wmsno, e.gdsid } into g
                               select new
                               {
                                   wmsno = g.Key.wmsno,
                                   gdsid = g.Key.gdsid,
                                   qty = g.Sum(e1 => e1.qty)
                               };
                var qryodrdtl = from e in WmsDc.odrdtl
                                join e1 in WmsDc.wms_bllmst on e.odrno equals e1.lnknewno
                                join e2 in qrytpdtl on new { e1.wmsno, e.gdsid } equals new { e2.wmsno, e2.gdsid }
                                where e1.wmsno == wmsno && e1.bllid == WMSConst.BLL_TYPE_REVIECEBLL && e.qty < e2.qty
                                select e;
                var arrqryodrdtl = qryodrdtl.ToArray();
                if (arrqryodrdtl.Length > 0)
                {
                    return RRInfo("I0449" ,arrqryodrdtl[0].gdsid );

                }
                StringBuilder sb = new StringBuilder();
                String cmdsql = null;
                //修改采购单实收数量                
                cmdsql = "update odrdtl set preqty=qty, qty=isnull(b.sumqty,0), pkgqty=isnull(b.sumqty,0), amt=convert(decimal(18,2),round(a.prc*isnull(b.sumqty,0), 4)), patamt=convert(decimal(18,2),round(a.taxprc*isnull(b.sumqty,0), 4)), taxamt=convert(decimal(18,2),round((a.taxprc*isnull(b.sumqty,0))-a.prc*isnull(b.sumqty,0), 4))   from "
                            + " odrdtl a "
                            + " left join "
                            + " ( "
                            + " select a1.wmsno, a1.gdsid, sum(a1.qty) sumqty ,b1.lnknewno from wms_blltp a1 "
                            + " 	inner join wms_bllmst b1 on a1.wmsno=b1.wmsno and a1.bllid=b1.bllid"
                            + " where b1.wmsno={0} and b1.bllid={1}"
                            + " group by a1.wmsno, a1.gdsid, b1.lnknewno	"
                            + " ) b on a.odrno=b.lnknewno and a.gdsid=b.gdsid "
                            + " where a.odrno in (select lnknewno from wms_bllmst where wmsno={2})";                
                sb.Append(cmdsql);                
                //设置收货单审核标志
                cmdsql = "update wms_bllmst set chkflg='" + GetY() + "', ckr={3}, chkdat={4} where wmsno={5} and bllid={6} ";
                sb.Append(cmdsql);
                //设置采购单收货标志
                cmdsql = "update odr set shflg='" + GetY() + "' from odr a inner join wms_bllmst b on b.lnknewno=a.odrno and b.lnknewbllid=a.bllid where b.wmsno={7} and b.bllid={8} ";
                sb.Append(cmdsql);
                
                //执行处理
                string sNow = GetCurrentDate();
                WmsDc.ExecuteCommand(sb.ToString(), new[] { wmsno, WMSConst.BLL_TYPE_REVIECEBLL, wmsno, LoginInfo.Usrid, sNow, wmsno, WMSConst.BLL_TYPE_REVIECEBLL, wmsno, WMSConst.BLL_TYPE_REVIECEBLL });
                try
                {
                    WmsDc.SubmitChanges();
                }
                catch (Exception ex)
                {
                    return RRErr(ex.Message, "E0066");

                }
                #endregion                

                #region 生成上架单
                //4.生成上架单
                //主表
                var qrytp = from e in WmsDc.wms_blltp
                            where e.wmsno == wmsno
                            && e.bllid == WMSConst.BLL_TYPE_REVIECEBLL
                            orderby e.tpcode, e.rcdidxtp
                            select e;
                wms_bllmst rmst = arrqry[0];
                wms_blldtl[] rdtl = arrqry1;
                wms_cang cwmst = new wms_cang();
                List<wms_cangdtl> lstcwdtl = new List<wms_cangdtl>();
                cwmst.wmsno = bllno;
                cwmst.bllid = WMSConst.BLL_TYPE_UPBLL;
                cwmst.savdptid = rmst.savdptid;
                cwmst.prvid = rmst.prvid;
                cwmst.qu = rmst.qu;
                /*mst.rcvdptid = rmst.rcvdptid;*/
                /*mst.times = rmst.times;
                mst.lnkbocino = rmst.lnkbocino;
                mst.lnkbocidat = rmst.lnkbocidat;*/
                cwmst.mkr = LoginInfo.Usrid;
                cwmst.mkedat = DateTime.Now.ToString("yyyyMMdd");
                cwmst.mkedat2 = GetCurrentDate();
                /*mst.ckr = rmst.ckr;*/
                cwmst.ckr = "";
                cwmst.chkflg = GetN();
                cwmst.chkdat = "";
                cwmst.opr = LoginInfo.Usrid;
                cwmst.brief = rmst.brief;
                cwmst.lnkbllid = rmst.bllid;
                cwmst.lnkno = rmst.wmsno;
                cwmst.lnkbrief = rmst.brief;
                //明细
                var qrycwdtl = from e in qrytp
                               join e1 in WmsDc.wms_blldtl on new { e.wmsno, e.bllid, e.gdsid } equals new { e1.wmsno, e1.bllid, e1.gdsid}
                               orderby e.rcdidx, e.rcdidxtp
                               select new
                               {
                                   e,
                                   e1
                               };
                int i = 1;
                foreach (var tp in qrycwdtl)
                {
                    wms_cangdtl cwdtl = new wms_cangdtl();
                    cwdtl.wmsno = bllno;
                    cwdtl.bllid = WMSConst.BLL_TYPE_UPBLL;
                    cwdtl.rcdidx = i++;
                    cwdtl.oldbarcode = "";
                    cwdtl.barcode = tp.e.barcode;
                    cwdtl.gdsid = tp.e.gdsid;
                    cwdtl.pkgid = tp.e.pkgid;
                    cwdtl.pkgqty = tp.e.qty;
                    cwdtl.qty = Math.Round(tp.e.qty, 4, MidpointRounding.AwayFromZero);
                    cwdtl.gdstype = tp.e.gdstype;
                    cwdtl.bthno = "";
                    cwdtl.vlddat = "";
                    cwdtl.bcd = tp.e1.bcd;
                    cwdtl.tpcode = tp.e.tpcode;
                    cwdtl.barcode = "";
                    cwdtl.bkr = "";
                    cwdtl.bokflg = GetN();
                    cwdtl.bokdat = "";
                    lstcwdtl.Add(cwdtl);
                }
                WmsDc.wms_cang.InsertOnSubmit(cwmst);
                WmsDc.wms_cangdtl.InsertAllOnSubmit(lstcwdtl);
                WmsDc.SubmitChanges();
                #endregion

                #region 推荐仓位
                /*String cmdsql1 = "declare @wmsno varchar(20) "
                                + " set @wmsno={0} "
                                + " exec SuggestBarcode @wmsno ";                
                WmsDc.ExecuteCommand(cmdsql1, new[] { wmsno });*/
                WmsDc.SuggestBarcode(wmsno);
                #endregion
                try
                {
                    WmsDc.SubmitChanges();
                    return RRSucc("成功", null, "S0218");

                }
                catch (Exception ex)
                {
                    return RRErr(ex.Message, "E0067");

                }

            });

            
        }

        /// <summary>
        /// 得到空仓号
        /// </summary>
        /// <param name="q">区位编码</param>
        /// <returns></returns>
        private string[] GetEmptyBarcodeByQu(string q)
        {
            var sfscprdid = GetCurrentFscprdid();
            var qry1 = from e in WmsDc.wms_cangwei
                       where e.qu == q
                       select new {e.barcode};
            var qry11 = from e in qry1
                        select e.barcode;

            var qry = from e in WmsDc.wms_gdsbs
                      where qry11.Contains(e.barcode.Trim())
                      && e.fscprdid == sfscprdid
                      group e by new { e.barcode, e.fscprdid } into g
                      select new
                      {
                          barcode = g.Key.barcode,
                          fscprdid = g.Key.fscprdid,
                          sumqty = g.Sum(g1 => Math.Pow(-1, g1.dbtcrt) * g1.qty)
                      };
            var qry2 = from e in qry1
                       join e1 in qry on e.barcode equals e1.barcode into allbarcode
                       from e1 in allbarcode.DefaultIfEmpty()
                       where e1.sumqty == 0 || e1.sumqty == null
                       select e.barcode.Trim();

            var arrqry = qry2.ToArray();

            return arrqry;
        }

        /// <summary>
        /// 得到托盘商品
        /// </summary>
        /// <param name="wmsno"></param>
        /// <param name="gdsid"></param>
        /// <returns></returns>
        [PWR(Pwrid = WMSConst.WMS_BACK_收货查询, pwrdes = "收货查询")]
        public ActionResult ListTpByGdsid(String wmsno, String gdsid, String gdstype)
        {
            gdsid = GetGdsidByGdsidOrBcd(gdsid);
            if (gdsid == null)
            {
                return RInfo( "I0227" );
            }

            var qrysh = from e in WmsDc.wms_blltp                        
                        where e.savdptid == LoginInfo.DefSavdptid
                        && e.gdsid == gdsid
                        /*&& e.gdstype == gdstype*/
                        && e.wmsno == wmsno
                        && e.bllid == WMSConst.BLL_TYPE_REVIECEBLL
                        select e;
            return RSucc("成功", qrysh.ToArray(), "S0113");            
        }

        #region MyRegion
        
        #endregion

        /// <summary>
        /// 确认商品收货 
        /// </summary>
        /// <param name="wmsno">收货单单号</param>
        /// <param name="gdsid">商品编码</param>
        /// <param name="gdstypes">单据商品序号</param>
        /// <param name="qtys">实收数量</param>
        /// <param name="tpcodes">托盘码</param>
        /// <param name="pkgids">包装ID</param>
        /// <returns>wms_blldtl, wms_blltp</returns>
        /// 如果 qtys,tpcodes,pkgids 不传参，就修改实收数量为0
        [PWR(Pwrid = WMSConst.WMS_BACK_收货确认, pwrdes = "收货确认")] 
        public ActionResult BokRecievGds(String wmsno, String gdsid, String gdstypes,  String qtys, String tpcodes, String pkgids)
        {
            using (TransactionScope scop = new TransactionScope())
            {
            gdsid = GetGdsidByGdsidOrBcd(gdsid);
            ////正在生成拣货单，请稍候重试
            //string quRetrv = GetQuByGdsid(gdsid, LoginInfo.DefStoreid).FirstOrDefault();
            //if (DoingRetrieve(LoginInfo.DefStoreid, quRetrv))
            //{
            //    return RInfo( "I0228" );
            //}

            if (gdsid == null)
            {
                return RInfo( "I0229" );
            }

            double[] qty = null;
            if (!String.IsNullOrEmpty(qtys))
            {
                String[] sqtys = qtys.Split(',');
                List<double> lstqtys = new List<double>();
                foreach (String s in sqtys)
                {
                    lstqtys.Add(double.Parse(s));
                }
                qty = lstqtys.ToArray();
            }
            String[] tpcode = !String.IsNullOrEmpty(tpcodes) ? tpcodes.Split(',') : null;
            String[] pkgid = !String.IsNullOrEmpty(pkgids) ? pkgids.Split(',') : null;
            String[] gdstype = gdstypes.Split(',');
            

                var qrymst = from e in WmsDc.wms_bllmst
                             where e.wmsno == wmsno && e.bllid == WMSConst.BLL_TYPE_REVIECEBLL
                            select e;
                //1.判断收货单是否已经审核，如果审核则退出
                var qry11 = from e in WmsDc.wms_blldtl
                            join e1 in WmsDc.wms_bllmst on new { e.wmsno, e.bllid } equals new { e1.wmsno, e1.bllid }
                            where e.wmsno == wmsno && e.bllid == WMSConst.BLL_TYPE_REVIECEBLL && (e.gdsid == gdsid) /*&& e.rcdidx == rcdidx*/
                            select e;
                var qry1 = from e in WmsDc.wms_blldtl
                           join e1 in WmsDc.wms_bllmst on new { e.wmsno, e.bllid } equals new { e1.wmsno, e1.bllid }
                           join e2 in WmsDc.bcd on e.gdsid equals e2.gdsid
                           where e.wmsno == wmsno && e.bllid==WMSConst.BLL_TYPE_REVIECEBLL && (e.gdsid == gdsid)/*&& e.rcdidx == rcdidx*/
                           select new { e, e1 };
                var qry = from e in qry1
                          select new
                           {
                               wmsno = e.e.wmsno.Trim(),
                               lnknewno = e.e1.lnknewno.Trim(),
                               e.e1.chkflg,
                               bllid = e.e1.bllid,
                               e.e1.qu,
                               gdsid = e.e.gdsid,
                               e.e.qty,
                               gdstype = e.e.gdstype,
                               rcdidx = e.e.rcdidx,
                               preqty = e.e.preqty,
                               mkr = e.e1.mkr
                           };
                var arrqry = qry.ToArray();
                

                //var arrqry11 = qry11.ToArray();
                foreach (wms_blldtl bdtl11 in qry11)
                {
                    bdtl11.qty = Math.Round(bdtl11.preqty, 4, MidpointRounding.AwayFromZero);
                }


                //0.删除商品审核
                var qrysh = from e in WmsDc.wms_blltp
                            where e.savdptid == LoginInfo.DefSavdptid
                            && e.qu == arrqry[0].qu
                            && e.gdsid == gdsid
                            && e.bllid == WMSConst.BLL_TYPE_REVIECEBLL
                            && e.wmsno == wmsno
                            /*&& e.rcdidx == rcdidx*/
                            select e;
                var arrqrysh = qrysh.ToArray();
                if (arrqrysh != null && arrqrysh.Length>0)
                {
                    WmsDc.wms_blltp.DeleteAllOnSubmit(arrqrysh);
                    iDelTpDtl(arrqrysh, qrymst.FirstOrDefault()); 
                }
                //WmsDc.SubmitChanges();

                //1.1.没有找到订单
                if (arrqry.Length <= 0)
                {
                    return RNoData("N0126");
                }
                //1.3.盘点是否是同一个操作人
                if (!IsSameLogin(arrqry[0].mkr))
                {
                    return RInfo("I0230", LoginInfo.Usrid, arrqry[0].mkr);
                }

                //1.2.订单已经审核
                /*if (arrqry[0].chkflg == GetY())
                {
                    return RRInfo("I0450" ,,);

                }*/
                //2.判断托盘参数和数量参数的数目是否一致
                List<wms_blltp> blltps = new List<wms_blltp>();
                if (!String.IsNullOrEmpty(tpcodes) && !String.IsNullOrEmpty(qtys) && qty!=null)    //如果托盘参数不为空
                {
                    if (qty.Length != tpcode.Length || pkgid.Length != qty.Length)
                    {
                        return RInfo( "I0231" );
                    }
                    //3.将商品导入托盘表                    
                    for (int i = 0; i < tpcode.Length; i++)
                    {
                        wms_blltp blltp = new wms_blltp();
                        blltp.wmsno = wmsno;
                        blltp.bllid = arrqry[0].bllid;
                        blltp.qu = arrqry[0].qu;
                        blltp.rcdidx = arrqry[0].rcdidx;
                        blltp.rcdidxtp = i;
                        blltp.gdsid = arrqry[0].gdsid;
                        blltp.pkgid = pkgid[i];
                        blltp.qty = Math.Round(qty[i], 4, MidpointRounding.AwayFromZero);
                        blltp.tpcode = tpcode[i];
                        blltp.savdptid = LoginInfo.DefSavdptid;
                        blltp.gdstype = gdstype[i];
                        blltp.bokflg = GetN();
                        blltp.bkr = "";
                        blltp.bokdat = "";
                        blltps.Add(blltp);
                    }

                    WmsDc.wms_blltp.InsertAllOnSubmit(blltps);
                }
                var qry2 = from e in qry1
                           select e.e;
                var arrqry2 = qry2.ToArray();
                //5.修改实收数量，并登帐
                arrqry2[0].qty = tpcodes != null ? Math.Round(blltps.Sum(s => s.qty), 4, MidpointRounding.AwayFromZero) : 0;
                arrqry2[0].bokflg = GetY();
                arrqry2[0].bokdat = DateTime.Now.ToString("yyyyMMddhhmmss");
                arrqry2[0].bkr = LoginInfo.Usrid;

                //6.判断实收是否大于应收
                if (arrqry2[0].qty > arrqry[0].preqty)
                {
                    return RInfo( "I0232" );
                }
                else
                {
                    String savdptid = LoginInfo.DefSavdptid;
                    //按照实收数量收货， 记日志
                    if (arrqry2[0].qty != arrqry[0].preqty)
                    {
                        Log.i(LoginInfo.Usrid, this.Mdlid, wmsno, WMSConst.BLL_TYPE_REVIECEBLL,
                            "收货审核", arrqry[0].gdsid.Trim() + ":应收:" + arrqry[0].preqty + ";实收:" + arrqry2[0].qty,
                            arrqry[0].qu, savdptid);
                    }

                }

                try
                {
                    WmsDc.SubmitChanges();
                    scop.Complete();
                    Object obj = new
                    {
                        wms_blldtl = arrqry2[0],
                        wms_blltp = blltps
                    };
                    return RSucc("成功", obj, "S0219");

                }
                catch (Exception ex)
                {                    
                    return RErr(ex.Message, "E0028");                    
                }
            }
        }

        private wms_blltp[] GetTp(String wmsno)
        {
            var qry1 = from e in WmsDc.wms_blltp
                       where e.bllid == WMSConst.BLL_TYPE_REVIECEBLL
                       && e.wmsno == wmsno
                       select e;
            var arrqry1 = qry1.ToArray();
            return arrqry1;
        }

        private wms_blldtl[] GetDtl(String wmsno)
        {
            var qryDtl = from e in WmsDc.wms_blldtl
                         where e.wmsno == wmsno && e.bllid == WMSConst.BLL_TYPE_REVIECEBLL
                         select e;
            wms_blldtl[] dtl = qryDtl.ToArray();
            return dtl;
        }

        private wms_bllmst GetMst(String wmsno)
        {
            var qryMst = from e in WmsDc.wms_bllmst
                         where e.wmsno == wmsno && e.bllid == WMSConst.BLL_TYPE_REVIECEBLL
                         && savdpts.Contains(e.savdptid) && qus.Contains(e.qu)
                         select e;
            wms_bllmst mst = qryMst.FirstOrDefault();
            return mst;
        }

        /// <summary>
        /// 修改收货单商品类型
        /// </summary>
        /// <param name="wmsno"></param>
        /// <param name="gdsid"></param>
        /// <param name="gdstype"></param>
        /// <returns></returns>
        [PWR(Pwrid = WMSConst.WMS_BACK_收货审核, pwrdes = "收货审核")]
        public ActionResult MdfyRecievGdsType(String wmsno, String gdsid, String gdstype, String newgdstype)
        {
            gdsid = GetGdsidByGdsidOrBcd(gdsid);
            ////正在生成拣货单，请稍候重试
            //string quRetrv = GetQuByGdsid(gdsid, LoginInfo.DefStoreid).FirstOrDefault();
            //if (DoingRetrieve(LoginInfo.DefStoreid, quRetrv))
            //{
            //    return RInfo( "I0233" );
            //}

            if (gdsid == null)
            {
                return RInfo( "I0234" );
            }

            /*var qry = from e in WmsDc.wms_blldtl
                      where e.wmsno == wmsno
                      && e.gdsid == gdsid
                      select e;
            var arrqry = qry.ToArray();
            if (arrqry.Length <= 0)
            {                
                return RNoData("N0127");                
            }
            wms_blldtl dtl = arrqry[0];*/
            var qry = from e in WmsDc.wms_blltp
                      where e.wmsno == wmsno
                      && e.bllid == WMSConst.BLL_TYPE_REVIECEBLL
                      && e.gdsid == gdsid
                      && e.gdstype == gdstype
                      select e;
            var arrqry = qry.ToArray();
            /*wms_blldtl[] dtls = GetDtl(wmsno).Where(e => e.gdsid == gdsid && e.gdstype == gdstype).ToArray();
            if (arrqry.Length <= 0 || dtls.Length <= 0)
            {
                return RNoData("N0128");
            }*/
            wms_blltp tp = arrqry[0];
            tp.gdstype = gdstype;
            /*wms_blldtl dtl = dtls.FirstOrDefault();
            dtl.gdstype = gdstype;*/
            try
            {
                WmsDc.SubmitChanges();

                return RSucc("成功", arrqry[0], "S0114");
            }
            catch (Exception ex)
            {
                return RErr(ex.Message, "E0029");
            }
        }

        protected override void SetModuleInfo()
        {
            this.Mdlid = "Reciev";
            this.Mdldes = "收货模块";
        }

        /// <summary>
        /// 收货查询
        /// </summary>
        /// <param name="begindat">查询开始时间</param>
        /// <param name="enddat">查询结束时间</param>
        /// <param name="wmsno">调整单单号/采购订单号</param>
        /// <param name="gdsid">商品货号、条码</param>
        /// <param name="barcode">仓位</param>
        /// <param name="barcode">供应商编号</param>
        /// <param name="barcode">业务库编码</param>
        /// <returns></returns>
        [PWR(Pwrid = WMSConst.WMS_BACK_收货制单, pwrdes = "收货制单")]
        public ActionResult FindBll(String begindat, String enddat, String wmsno, String gdsid, String barcode, String prvid, String dptid)
        {
            //判断分区是否有效
            if (!String.IsNullOrEmpty(barcode) && !IsExistBarcode(barcode))
            {
                return RInfo( "I0235",barcode.Trim()  );
            }
            var arrqrymst = FindBllFromBllMst101(WMSConst.BLL_TYPE_REVIECEBLL, begindat, enddat, wmsno, gdsid, barcode,prvid, dptid);            
                      
            if (arrqrymst.Length <= 0)
            {
                return RNoData("N0129");
            }
            return RSucc("成功", arrqrymst, "S0115");
        }
    }
}
