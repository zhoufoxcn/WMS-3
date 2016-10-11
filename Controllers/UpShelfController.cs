﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WMS.Models;
using System.Transactions;
using System.Web.Script.Serialization;

namespace WMS.Controllers
{
    /// <summary>
    /// 上架模块（整单上架）
    /// </summary>
    public class UpShelfController : SsnController
    {

        /// <summary>
        /// 初始化Initialize
        /// </summary>
        /// <param name="requestContext"></param>
        protected override void Initialize(RequestContext requestContext)
        {
            Mdlid = "UpShelf";
            Mdldes = "上架模块";

            base.Initialize(requestContext);

            //1.检查是否登录
            if (!CheckLogin())
            {                
                Response.ContentType = "application/json";
                Response.Write("{\"ResultCode\":\"-1\", \"ResultDesc\":\"尚未登录\"}");
                Response.End();
                return;
            }
        }

        /// <summary>
        /// 得到当天的上架单
        /// </summary>
        /// <returns></returns>
        [PWR(Pwrid = WMSConst.WMS_BACK_上架查询, pwrdes = "上架查询")]
        public ActionResult GetCurrentDayUpShelf()
        {
            String Today = GetCurrentDay();

            var qry = from e in WmsDc.wms_cang
                      join e1 in WmsDc.emp on e.mkr equals e1.empid
                      where e.bllid == WMSConst.BLL_TYPE_UPBLL
                      && e.mkedat == Today
                      && e.savdptid == LoginInfo.DefSavdptid
                      && qus.Contains(e.qu.Trim())
                      select new
                      {
                          e.bllid,
                          e.brief,
                          e.chkdat,
                          e.chkflg,
                          e.ckr,
                          e.lnkbllid,
                          e.lnkbocidat,
                          e.lnkbocino,
                          e.lnkbrief,
                          e.lnkno,
                          e.mkedat,
                          e.mkr,
                          e.opr,
                          e.prvid,
                          e.qu,
                          e.rcvdptid,
                          e.savdptid,
                          e.times,
                          e.wmsno,
                          mkrdes = e1.empdes
                      };
            var arrqry = qry.ToArray();
            if (arrqry.Length <= 0)
            {
                return RNoData("N0206", Today);
            }
            return RSucc("成功", arrqry, "S0192");
        }

        /// <summary>
        /// 得到当天上架单明细
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCurrentDayUpShelfDetail()
        {
            String Today = GetCurrentDay();

            var qrymst = from e in WmsDc.wms_cang
                      where e.bllid == WMSConst.BLL_TYPE_UPBLL
                      && e.mkedat == Today
                      && e.savdptid == LoginInfo.DefSavdptid
                      && qus.Contains(e.qu.Trim())
                      select e.wmsno;
            var arrqrymst = qrymst.ToArray();
            if (arrqrymst.Length <= 0)
            {
                return RNoData("N0207", Today);
            }

            var qrydtl = from e in WmsDc.wms_cangdtl
                         where arrqrymst.Contains(e.wmsno.Trim()) && e.bllid == WMSConst.BLL_TYPE_UPBLL
                         group e by new { e.tpcode, e.barcode, e.bokflg, e.wmsno } into g                         
                         select new
                         {
                             g.Key.wmsno,
                             g.Key.tpcode,
                             g.Key.barcode,
                             g.Key.bokflg
                         };
            qrydtl = qrydtl.OrderBy(e => e.barcode).OrderBy(e => e.tpcode).OrderBy(e => e.wmsno);
            var arrqrydtl = qrydtl.ToArray();
            if (arrqrydtl.Length <= 0)
            {
                return RNoData("N0208");
            }

            return RSucc("成功", arrqrydtl, "S0193");            
        }

        /// <summary>
        /// 得到上架单托盘明细
        /// </summary>
        /// <param name="wmsno"></param>
        /// <returns></returns>
        [PWR(Pwrid = WMSConst.WMS_BACK_上架查询, pwrdes = "上架查询")]
        public ActionResult GetUpBllDtlByBarcode(String wmsno, String tp)
        {
            var qry = from e in WmsDc.wms_cang
                      where e.bllid == WMSConst.BLL_TYPE_UPBLL
                      && (e.savdptid == LoginInfo.DefSavdptid || e.savdptid == LoginInfo.DefCsSavdptid)
                      && qus.Contains(e.qu.Trim())
                      && e.wmsno == wmsno
                      select e;
            var arrqry = qry.ToArray();
            if (arrqry.Length <= 0)
            {
                return RNoData("N0209", wmsno);
            }
            wms_cang mst = arrqry[0];

            var qrydtltp = from e in qry
                           join e1 in WmsDc.wms_bllmst on new { wmsno = e.lnkno, bllid = e.lnkbllid } equals new { e1.wmsno, e1.bllid }
                           join e2 in WmsDc.wms_blltp on new { wmsno = e1.wmsno, bllid = e1.bllid } equals new { e2.wmsno, e2.bllid }
                           where e2.tpcode.Contains(tp.Trim())
                           group e2 by new { e2.gdsid, e2.gdstype, e2.pkgid, e2.tpcode, e2.savdptid } into g
                           select new
                           {
                               g.Key.gdsid,
                               g.Key.gdstype,
                               g.Key.pkgid,
                               g.Key.tpcode,
                               g.Key.savdptid,
                               sqty = g.Sum(e => e.qty)
                           };
            var qrydtltpgds = from e in qrydtltp
                              join e1 in WmsDc.gds on e.gdsid equals e1.gdsid
                              select new
                              {
                                  e.gdsid,
                                  e.gdstype,
                                  e.pkgid,
                                  e.savdptid,
                                  e.tpcode,
                                  e.sqty,
                                  e1.gdsdes,
                                  e1.spc,
                                  e1.bsepkg
                              };

            var arrqrydtl = qrydtltpgds.ToArray();
            if (arrqrydtl.Length <= 0)
            {
                return RNoData("N0210");
            }

            return RSucc("成功", arrqrydtl, "S0194");
        }

        /// <summary>
        /// 得到上架单明细
        /// </summary>
        /// <param name="wmsno"></param>
        /// <returns></returns>
        [PWR(Pwrid = WMSConst.WMS_BACK_上架查询, pwrdes = "上架查询")]
        public ActionResult GetUpBllDtl(String wmsno)
        {
            var qry = from e in WmsDc.wms_cang
                      where e.bllid == WMSConst.BLL_TYPE_UPBLL
                      && e.savdptid == LoginInfo.DefSavdptid
                      && qus.Contains(e.qu.Trim())
                      && e.wmsno == wmsno
                      select e;
            var arrqry = qry.ToArray();            
            if (arrqry.Length <= 0)
            {
                return RNoData("N0211",wmsno);
            }
            wms_cang mst = arrqry[0];

            var qrydtl = from e in WmsDc.wms_cangdtl
                         where e.wmsno == wmsno && e.bllid == WMSConst.BLL_TYPE_UPBLL
                         group e by new { e.tpcode, e.barcode, e.bokflg, e.wmsno } into g
                         select new
                         {
                             g.Key.wmsno,
                             g.Key.tpcode,
                             g.Key.barcode,
                             g.Key.bokflg
                         };
            var arrqrydtl = qrydtl.ToArray();
            if (arrqrydtl.Length <= 0)
            {
                return RNoData("N0212");
            }

            return RSucc("成功", arrqrydtl, "S0195");
        }
        
        /// <summary>
        /// 扫描仓位码和托盘码，确定上架是否正确
        /// </summary>
        /// <param name="wmsno">上架单号</param>
        /// <param name="barcode">仓位码</param>
        /// <param name="newbarcode">修改的仓位码</param>
        /// <param name="tpcode">托盘码</param>
        /// <returns></returns>
        [PWR(Pwrid = WMSConst.WMS_BACK_上架确认, pwrdes = "上架确认")]
        public ActionResult CheckCangwei(String wmsno, String barcode, String newbarcode, String tpcode)
        {
            using (TransactionScope scop = new TransactionScope(TransactionScopeOption.Required, options))
            {
                wms_cangwei oldCw = null;
                String rqu = GetQuByBarcode(newbarcode);
                String oqu = GetQuByBarcode(barcode);

                //如果旧仓位不为空，就需要查询出就仓位，以便修改其仓位托盘tpflg标志
                if (!string.IsNullOrEmpty(barcode.Trim()))
                {
                    //判断仓位码是否有效                                
                    if (!IsExistBarcode(barcode))
                    {
                        return RInfo("I0396", barcode.Trim());
                    }
                    oldCw = GetCangWei(barcode);
                }

                //判断仓位码是否有效
                if (!string.IsNullOrEmpty(newbarcode) && !IsExistBarcode(newbarcode))
                {
                    return RInfo("I0397", newbarcode.Trim());
                }
                //todo 判断新的仓位码是否为今天已经上架单中已有的仓位

                //主表
                wms_cang cang = GetCangMst(wmsno);

                //明细表
                var qrydtl = GetCangDtl(wmsno)
                             .Where(e => e.barcode.Trim() == barcode.Trim() && e.tpcode.Trim() == tpcode.Trim());
                var arrqrydtl = qrydtl.ToArray();
                // 判断明细的分区是否是同一个分区，而且newbarcode的分区也在商品的分区内
                string initQu = null;
                foreach (wms_cangdtl dtl in arrqrydtl)
                {
                    string gdsqu = GetQuByGdsid(dtl.gdsid, LoginInfo.DefStoreid).FirstOrDefault();
                    if (string.IsNullOrEmpty(initQu))
                    {
                        initQu = gdsqu;
                    }
                    if (initQu != gdsqu)
                    {
                        return RInfo("I0398");
                    }
                }
                if (!dtqus.Contains(rqu))
                {
                    if (initQu != newbarcode.Substring(0, 2))
                    {
                        return RInfo("I0399");
                    }
                }

                #region 有效性检查
                //检查单号是否有效            
                if (cang == null)
                {
                    return RNoData("N0213");
                }
                ////正在生成拣货单，请稍候重试
                //string quRetrv = cang.qu;
                //if (DoingRetrieve(LoginInfo.DefStoreid, quRetrv))
                //{
                //    return RInfo( "I0400" );
                //}

                //检查是否已经审核
                if (cang.chkflg == GetY())
                {
                    return RInfo("I0401");
                }

                //未查找到托盘的正确仓位
                if (arrqrydtl.Length <= 0)
                {
                    return RNoData("N0214");
                }

                //检查同一个托盘是否都已经上架            
                if (arrqrydtl[0].bokflg == GetY())
                {
                    return RInfo("I0402");
                }
                #endregion

                wms_cangwei cw = null;
                //如果调入不在堆头区，就判断一下区域间能不能互相调用
                //if (!dtqus.Contains(rqu))
                //{
                //查看新仓位是否不为推荐仓位                
                if (!String.IsNullOrEmpty(newbarcode) && barcode.Trim() != newbarcode.Trim())
                {
                    cw = GetCangWei(newbarcode);
                    if (cw == null)
                    {
                        return RNoData("N0215", newbarcode);
                    }

                    if (!savdpts.Contains(cw.savdptid.Trim()))
                    {
                        return RInfo("I0403", newbarcode);
                    }
                    if (!qus.Contains(cw.qu.Trim()))
                    {
                        return RInfo("I0404", newbarcode);
                    }


                    /*var qrynewcw = from e in WmsDc.wms_cang
                                   join e1 in WmsDc.wms_cangdtl on new { e.wmsno, e.bllid } equals new { e1.wmsno, e1.bllid }
                                   where //e.wmsno == wmsno && 
                                   e.bllid == "102" //上架单
                                   && e1.barcode == newbarcode
                                   && e.mkedat.Substring(0, 8) == GetCurrentDay()
                                   select e;
                    if (oldCw != null &&
                        (
                        !qrynewcw.Any() &&  //这两句不能换位置
                        cw.tjflg == GetY() //调整仓位{0}为不在当天上架单中的推荐仓位，不能使用                        
                        )
                        )
                    {
                        return RInfo("I0405", newbarcode);
                    }*/
                    //修改新仓位标记tpflg=GetY()
                    cw.tpflg = GetY();
                    //修改旧仓位tpflg=GetN()
                    if (oldCw != null)
                    {
                        oldCw.tpflg = GetN();
                    }
                    WmsDc.SubmitChanges();
                }
                //}

                #region 上架托盘到仓位
                foreach (wms_cangdtl cangdtl in arrqrydtl)
                {
                    cangdtl.bkr = LoginInfo.Usrid;
                    cangdtl.bokflg = GetY();
                    cangdtl.bokdat = DateTime.Now.ToString("yyyyMMddHHmmss");
                    if (cw != null)
                    {
                        cangdtl.oldbarcode = cangdtl.barcode;
                        cangdtl.barcode = newbarcode;
                    }
                }

                try
                {
                    WmsDc.SubmitChanges();

                    //done: 判断是否已经上架单明细已经审核完毕，完毕后就直接审核整单
                    //明细表
                    /**/                    
                    int? isLasted = WmsDc.ExecuteQuery<int?>("select count(*) from wms_cangdtl with(updlock) where wmsno={0} and bllid={1} and bokflg={2}",
                        wmsno, WMSConst.BLL_TYPE_UPBLL, GetN()).FirstOrDefault();
                    if (isLasted==null || isLasted == 0)
                    {
                        wms_cangdtl[] arrqryaldtl = GetCangDtl(wmsno)
                                                .Where(e => e.bokflg == GetN())
                                                .ToArray();
                        // done: 判断盘点收货单有没有整单审核，提示不能上架                        
                        wms_bllmst bllmst = GetBllMst(wmsno);
                        if (bllmst == null)
                        {
                            return RInfo("I0406");
                        }
                        if (bllmst.chkflg == GetN())
                        {
                            return RInfo("I0407");
                        }
                        #region 审核整单
                        JsonResult ar = (JsonResult)AdtUpShelf(wmsno);
                        ResultMessage rm = (ResultMessage)ar.Data;
                        if (rm.ResultCode != ResultMessage.RESULTMESSAGE_SUCCESS)
                        {
                            return ar;
                        }
                        #endregion 审核整单
                    }

                    scop.Complete();
                    return RSucc("扫描仓位码和托盘码匹配成功！", null, "S0196");
                }
                catch (Exception ex)
                {
                    return RErr("异常错误！" + ex.Message, "E0053");
                }
                #endregion
            }
        }

        private wms_cangwei GetCangWei(String newbarcode)
        {
            var qrytj = from e in WmsDc.wms_cangwei
                        where e.isvld == GetY()// && e.tjflg == GetN()
                            //&& savdpts.Contains(e.savdptid)
                            //&& qus.Contains(e.qu)
                        && e.barcode == newbarcode
                        select e;
            wms_cangwei cw = qrytj.FirstOrDefault();
            return cw;
        }

        private wms_cangdtl[] GetCangDtl(String wmsno)
        {
            var qryaldtl = from e in WmsDc.wms_cangdtl
                           join e1 in WmsDc.wms_cang on new { e.wmsno, e.bllid } equals new { e1.wmsno, e1.bllid }
                           where e.wmsno == wmsno
                           && e1.bllid == WMSConst.BLL_TYPE_UPBLL
                           //&& e.bokflg == GetN()
                           select e;
            var arrqryaldtl = qryaldtl.ToArray();
            return arrqryaldtl;
        }

        private wms_bllmst GetBllMst(String wmsno)
        {
            var qry = from e in WmsDc.wms_bllmst
                      join e1 in WmsDc.wms_cang on new { e.wmsno, e.bllid } equals new { wmsno = e1.lnkno, bllid = e1.lnkbllid }
                      where e1.wmsno == wmsno && e1.bllid == WMSConst.BLL_TYPE_UPBLL
                      select e;
            wms_bllmst bllmst = qry.FirstOrDefault();
            return bllmst;
        }

        private ActionResult AdtUpShelf(String wmsno)
        {

            //主表
            wms_cang cang = GetCangMst(wmsno);

            //明细表            
            wms_cangdtl[] arrqrydtl = GetCangDtl(wmsno);

            #region 有效性检查
            //检查单号是否有效            
            if (cang==null)
            {
                return RNoData("N0216");
            }
            ////正在生成拣货单，请稍候重试
            //string quRetrv = cang.qu;
            //if (DoingRetrieve(LoginInfo.DefStoreid, quRetrv))
            //{
            //    return RInfo( "I0408" );
            //}

            //检查是否已经审核            
            if (cang.chkflg == GetY())
            {
                return RInfo( "I0409" );
            }

            //未查找到收货单明细
            if (arrqrydtl.Length <= 0)
            {
                return RNoData("N0217");
            }

            //检查是否所有托盘都已经确认
            foreach (wms_cangdtl cangdtl in arrqrydtl)
            {
                if (cangdtl.bokflg == GetN())
                {
                    return RInfo( "I0410",cangdtl.tpcode  );
                }
            }
            #endregion

            #region 登帐
            String fscprdid = GetCurrentFscprdid();
            var qrygdsbs = from e in WmsDc.wms_gdsbs
                           where e.fscprdid == fscprdid
                           && e.srcbllno == cang.wmsno
                           && e.bllid == cang.bllid
                           select e;
            var arrqrygdsbs = qrygdsbs.ToArray();
            WmsDc.wms_gdsbs.DeleteAllOnSubmit(arrqrygdsbs);
            WmsDc.SubmitChanges();
            
            List<wms_gdsbs> lstgdsbs = new List<wms_gdsbs>();
            foreach (wms_cangdtl cangdtl in arrqrydtl)
            {
                wms_gdsbs gdsbs = new wms_gdsbs();
                gdsbs.actdat = DateTime.Now.ToString("yyyyMMdd");
                gdsbs.barcode = cangdtl.barcode;
                gdsbs.bcd = cangdtl.bcd;
                gdsbs.bllid = cang.bllid;
                gdsbs.brief = cang.brief;
                gdsbs.bthno = cangdtl.bthno;
                gdsbs.dbtcrt = '1';
                gdsbs.empid = LoginInfo.Usrid;
                gdsbs.fscprdid = fscprdid;
                gdsbs.gdsid = cangdtl.gdsid;
                gdsbs.gdstype = cangdtl.gdstype;
                gdsbs.prvid = cang.prvid;
                gdsbs.qty = cangdtl.qty;
                gdsbs.savdptid = cang.savdptid;
                gdsbs.srcbllno = cang.wmsno;
                gdsbs.srcrcdidx = cangdtl.rcdidx;
                gdsbs.vlddat = cangdtl.vlddat;
                lstgdsbs.Add(gdsbs);

                #region 统计帐表数据更新                
                wms_cangwei cw = GetCangWei(cangdtl.barcode);
                if (cw == null)
                {
                    return RNoData("N0218", gdsbs.gdsid);
                }                

                #region 修改仓位有货标记
                /*try       //周熙写了触发器，说不用改这个了
                {
                    cw.kcflg = WMSConst.KC_FLG_HASQTY;     //修改仓位有无库存，有为1，没有为0；
                    cw.tpflg = GetN();
                    WmsDc.SubmitChanges();
                }
                catch (Exception ex)
                {
                }*/
                #endregion

                var qrycwgdsbs = from e in WmsDc.wms_cwgdsbs
                                 where e.barcode == gdsbs.barcode.Trim()
                                 //&& e.bcd == gdsbs.bcd
                                 //&& e.bthno == gdsbs.bthno
                                 && e.gdsid == gdsbs.gdsid.Trim()
                                 && e.gdstype == gdsbs.gdstype.Trim()
                                 && e.bthno == gdsbs.bthno
                                 && e.vlddat == gdsbs.vlddat
                                 //&& e.prvid == gdsbs.prvid
                                 && e.qu == cw.qu.Trim()
                                 select e;
                var arrqrycwgdsbs = qrycwgdsbs.ToArray();

                //如果统计帐表里面没有就增加一条记录
                if (arrqrycwgdsbs.Length <= 0)
                {
                    wms_cwgdsbs cwgdsbs = new wms_cwgdsbs();
                    cwgdsbs.barcode = gdsbs.barcode;
                    cwgdsbs.bcd = gdsbs.bcd;                    
                    cwgdsbs.gdsid = gdsbs.gdsid;
                    cwgdsbs.qty = gdsbs.qty;
                    cwgdsbs.gdstype = gdsbs.gdstype;
                    cwgdsbs.prvid = gdsbs.prvid;
                    cwgdsbs.qu = cw.qu;
                    cwgdsbs.savdptid = gdsbs.savdptid;
                    cwgdsbs.bthno = gdsbs.bthno == null ? "1" : gdsbs.bthno;
                    cwgdsbs.vlddat = gdsbs.vlddat == null ? GetCurrentDay() : gdsbs.vlddat;

                    WmsDc.wms_cwgdsbs.InsertOnSubmit(cwgdsbs);
                    WmsDc.SubmitChanges();
                }
                else    //如果有的话就修改统计帐表的信息
                {
                    wms_cwgdsbs cwgdsbs = arrqrycwgdsbs[0];
                    cwgdsbs.qty += gdsbs.qty;
                    WmsDc.SubmitChanges();
                }
                #endregion

                //WmsDc.SubmitChanges();
            }
            //WmsDc.wms_gdsbs.InsertAllOnSubmit(lstgdsbs);            
            #endregion

            #region 修改审核标记
            cang.chkflg = GetY();
            cang.ckr = LoginInfo.Usrid;
            cang.chkdat = DateTime.Now.ToString("yyyyMMddHHmmss");
            #endregion

            WmsDc.SubmitChanges();

            return RSucc("上架单审核成功！", null, "S0197");
        }

        private wms_cang GetCangMst(String wmsno)
        {
            var qrymst = from e in WmsDc.wms_cang
                         where e.wmsno == wmsno && e.bllid == WMSConst.BLL_TYPE_UPBLL
                         select e;
            wms_cang cang = qrymst.FirstOrDefault();
            return cang;
        }

        /// <summary>
        /// 审核上架单
        /// </summary>
        /// <param name="wmsno">上架单号</param>
        /// <returns></returns>
        [PWR(Pwrid = WMSConst.WMS_BACK_上架审核, pwrdes = "上架审核")]
        public ActionResult AuditUpShelf(String wmsno)
        {
            using (TransactionScope scop = new TransactionScope(TransactionScopeOption.Required, options))
            {
                JsonResult ar = (JsonResult)AdtUpShelf(wmsno);
                ResultMessage rm = (ResultMessage)ar.Data;
                if (rm.ResultCode != ResultMessage.RESULTMESSAGE_SUCCESS)
                {
                    return ar;
                }
                try
                {                    
                    
                    scop.Complete();
                    
                    return RSucc("上架单审核成功！", null, "S0198");
                }
                catch (Exception ex)
                {                    
                    return RErr("异常错误！" + ex.Message, "E0054");
                }
            }
        }

        /// <summary>
        /// 上架查询
        /// </summary>
        /// <param name="begindat">查询开始时间</param>
        /// <param name="enddat">查询结束时间</param>
        /// <param name="wmsno">调整单单号</param>
        /// <param name="gdsid">商品货号、条码</param>
        /// <param name="barcode">仓位</param>
        /// <returns></returns>
        [PWR(Pwrid = WMSConst.WMS_BACK_上架查询, pwrdes = "上架查询")]
        public ActionResult FindBll(String begindat, String enddat, String wmsno, String gdsid, String barcode)
        {
            //判断分区是否有效
            if (!String.IsNullOrEmpty(barcode) && !IsExistBarcode(barcode))
            {
                return RInfo( "I0411",barcode.Trim()  );
            }

            var arrqrymst = FindBllFromCangMst(WMSConst.BLL_TYPE_UPBLL, begindat, enddat, wmsno, gdsid, barcode);
            if (arrqrymst.Length <= 0)
            {
                return RNoData("N0219");
            }
            return RSucc("成功", arrqrymst, "S0199");
        }

        protected override void SetModuleInfo()
        {
            this.Mdlid = "UpShelf";
            this.Mdldes = "上架模块";
        }
    }
}
