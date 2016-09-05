﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WMS.Models;
using System.Net;
using System.IO;
using WMS.Common;
using System.Transactions;

namespace WMS.Controllers
{
    /// <summary>
    /// 播种模块
    /// </summary>
    public class BoZController : SsnController
    {
        /// <summary>
        /// 设置模块信息
        /// </summary>
        protected override void SetModuleInfo()
        {
            
            Mdlid = "BoZ";
            Mdldes = "播种模块";
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="requestContext">HTTP请求对象</param>
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);            
        }

        /// <summary>
        /// 检查wmsno的单号是否已经审核
        /// </summary>
        /// <param name="bzmst">主单</param>
        /// <returns></returns>
        private bool ChkHasAdt(wms_bzmst bzmst)
        {
            return bzmst.chkflg == GetY();
        }

        /// <summary>
        /// 审核播种商品
        /// </summary>
        /// <param name="wmsno">播种单单号</param>
        /// <param name="stkouno">配送单单号</param>
        /// <param name="gdsid">商品货号</param>      
        /// <param name="rcvdptid">发送分店</param>
        /// <param name="qty">实际播种数量</param>
        /// <param name="rcdidx">配送中单据中的序号</param>
        /// <returns></returns>
        [PWR(Pwrid = WMSConst.WMS_BACK_播种确认, pwrdes = "播种确认")]
        public ActionResult BokBozBllGds(String wmsno, String stkouno, String rcvdptid, String gdsid, double qty, int? rcdidx)
        {
            
            using (TransactionScope scop = new TransactionScope(TransactionScopeOption.Required, options))
            {
                //i(wmsno, "", System.DateTime.Now.ToString("yyyyMMddHHmmss.fff"), Request["rnd"], "1", LoginInfo.DefSavdptid);
                gdsid = GetGdsidByGdsidOrBcd(gdsid);                

                //正在生成拣货单，请稍候重试
                string quRetrv = GetQuByGdsid(gdsid, LoginInfo.DefStoreid).FirstOrDefault();
                //if (DoingRetrieve(LoginInfo.DefStoreid, quRetrv))
                //{
                //    return RInfo( "I0077" );
                //}

                if (gdsid == null)
                {
                    return RInfo( "I0078" );
                }

                String Dat = GetCurrentDay();

                /*
                 var qry = from e in WmsDc.stkot
                          from e1 in e.stkotdtl
                          join e2 in WmsDc.gds on e1.gdsid equals e2.gdsid
                          where e.wmsno == wmsno
                          && e.bllid == WMSConst.BLL_TYPE_DISPATCH
                          && e.wmsbllid == WMSConst.BLL_TYPE_RETRIEVE
                          && dpts.Contains(e.dptid)
                          && e.savdptid == LoginInfo.DefSavdptid
                          && e.rcvdptid == rcvdptid                      
                          && e1.gdsid == gdsid                      
                          && e1.rcdidx == rcdidx
                          select e;
                */

                var qry = from e in WmsDc.stkot
                          where e.stkouno == stkouno
                          && e.bllid == WMSConst.BLL_TYPE_DISPATCH
                          && e.wmsbllid == WMSConst.BLL_TYPE_RETRIEVE
                          && dpts.Contains(e.dptid.Trim())
                          && (e.savdptid == LoginInfo.DefSavdptid || e.savdptid == LoginInfo.DefCsSavdptid)
                          && e.rcvdptid == rcvdptid
                          select e;
                var arrqry = qry.ToArray();
                if (arrqry.Length <= 0)
                {
                    return RNoData("N0048");
                }
                var stkotgds = arrqry[0];
                if (wmsno == null)
                {
                    wmsno = stkotgds.wmsno;
                }
                if (stkotgds.chkflg == GetY())
                {
                    return RInfo( "I0079" );
                }
                /*if (stkotgds.bzflg == GetY())
                {
                    return RInfo( "I0080" );
                }*/
                var qrydtl = from e in stkotgds.stkotdtl
                             where e.gdsid.Trim() == gdsid.Trim() && e.rcdidx == rcdidx
                             select e;
                var arrqrydtl = qrydtl.ToArray();
                if (arrqrydtl.Length <= 0)
                {
                    return RNoData("N0049");
                }
                stkotdtl stkdtl = arrqrydtl[0];
                double? preqty = stkdtl.preqty;
                if (stkdtl.preqty == null)              ///如果应收数量为空，就把qty中的数量填入其中
                {
                    stkdtl.preqty = stkdtl.qty;
                    preqty = stkdtl.qty;
                }
                if (preqty < qty)       //如果实收数量大于应收数量就退出
                {
                    return RInfo( "I0081" );
                }
                if (preqty != qty)
                {
                    GetRealteQuResult qu = GetRealteQu(stkotgds.dptid, LoginInfo.DefSavdptid);
                    Log.i(LoginInfo.Usrid, Mdlid, stkotgds.stkouno, stkotgds.bllid, "播种审核",
                        gdsid.Trim() + ":应播:" + preqty + ";实播:" + qty,
                            qu.qu, qu.savdptid);
                }

                //查看该商品是否已经被非本人确认
                if (stkdtl.bzflg == GetY() && stkdtl.bzr != LoginInfo.Usrid)
                {
                    return RInfo( "I0082",stkdtl.bzr  );
                }

                #region 检查参数有效性
                if (arrqry == null)
                {
                    return RInfo( "I0083" );
                }
                if (stkdtl == null)
                {

                    return RInfo( "I0084" );
                }

                #endregion

                //修改审核标记
                try
                {
                    /*
                        * preqty = preqty==null ? qty : preqty
                        * 
                        * 公式：taxamt = qty*prc*taxrto
                        * amt = qty*prc
                        * salamt = qty*salprc
                        * patamt = qty*taxprc
                        * stotcstamt = qty*stotcstprc
                        * 
                    */
                    stkdtl.qty = Math.Round(qty, 4, MidpointRounding.AwayFromZero);
                    stkdtl.pkgqty = Math.Round(qty, 4, MidpointRounding.AwayFromZero);
                    stkdtl.bzdat = GetCurrentDate();
                    stkdtl.bzr = LoginInfo.Usrid;
                    stkdtl.bzflg = GetY();
                    stkdtl.taxamt = Math.Round(qty * stkdtl.prc * stkdtl.taxrto, 4);
                    stkdtl.amt = Math.Round(qty * stkdtl.prc, 4);
                    stkdtl.salamt = qty * stkdtl.salprc;
                    stkdtl.patamt = Math.Round(qty * stkdtl.taxprc, 4);
                    stkdtl.stotcstamt = Math.Round(qty * stkdtl.stotcstprc.Value, 4);

                    //判断改单据是否已经全部商品已经确认，全部确认后的，实收商品总数和应收商品总数相同就直接修改主单的审核标记
                    /*double? sqty = stkotgds
                                    .stkotdtl
                                    .Where(e=>e.bzflg==GetY())
                                    .Sum(e=>e.qty==null?0:e.qty);
                    double? spreqty = stkotgds.stkotdtl.Sum(e=>e.preqty==null?e.qty:e.preqty);
                    if(sqty==spreqty){
                        stkotgds.chkflg = GetY();
                        stkotgds.chkdat = Dat;
                        stkotgds.ckr = LoginInfo.Usrid;
                        stklst astklst = new stklst();
                        astklst.stkouno = stkotgds.stkouno;
                        WmsDc.stklst.InsertOnSubmit(astklst);
                    }*/

                    WmsDc.SubmitChanges();

                    ///如果明细全部播种完
                    ///就修改审核标记
                    ///和播种标记
                    double sqtycnt = stkotgds
                                     .stkotdtl
                                     .Where(e => e.bzflg == GetY() && Math.Round(e.qty, 2, MidpointRounding.AwayFromZero) != 0)
                                     .Count();
                    double spreqtycnt = stkotgds
                                        .stkotdtl
                                        .Where(e => Math.Round(e.qty, 2, MidpointRounding.AwayFromZero) != 0)
                                        .Count();
                    d(wmsno, WMSConst.BLL_TYPE_UPBLL, "审核播种商品", "sqtycnt=" + sqtycnt + "&spreqtycnt=" + spreqtycnt, "", LoginInfo.DefSavdptid);
                    if (sqtycnt == spreqtycnt)
                    {
                        CkBzFlg(stkotgds);

                        //查看有没有明细为空的单据，直接修改播种标记
                        var qryZeroBz = from e in WmsDc.stkotdtl
                                        where e.stkot.wmsno == wmsno && e.stkot.wmsbllid == WMSConst.BLL_TYPE_RETRIEVE
                                        group e by e.stkouno into g
                                        select new
                                        {
                                            stkouno = g.Key,
                                            sqty = g.Sum(e => e.qty)
                                        };
                        qryZeroBz = qryZeroBz.Where(e => e.sqty == 0);
                        var qryZeroBzmst = from e in WmsDc.stkot
                                           join e1 in qryZeroBz on e.stkouno equals e1.stkouno
                                           where e.chkflg != GetY()
                                           select e;
                        foreach (var q in qryZeroBzmst)
                        {
                            CkBzFlg(q);
                            foreach (var dl in q.stkotdtl)
                            {
                                dl.bzflg = GetY();
                                dl.bzdat = GetCurrentDate();
                                dl.bzr = LoginInfo.Usrid;
                            }
                        }

                    }

                    //i(wmsno, "", System.DateTime.Now.ToString("yyyyMMddHHmmss.fff"), Request["rnd"], "2", LoginInfo.DefSavdptid);

                    WmsDc.SubmitChanges();
                    
                    scop.Complete();
                    return RSucc("成功", null, "S0038");
                }
                catch (Exception ex)
                {                    
                    return RErr(ex.Message, "E0012");
                }
            }
        }

        private string CanBozByRcvdptid(string wmsno, string stkouno, string rcvdptid, string gdsid, double qty, int? rcdidx)
        {
            //得到该商品未播种的分店排序列表
            var qry = from e in WmsDc.stkot
                      join e1 in WmsDc.stkotdtl on e.stkouno equals e1.stkouno
                      join e2 in WmsDc.gds on e1.gdsid equals e2.gdsid
                      join e3 in WmsDc.wms_cang on new { e.wmsno, e.wmsbllid } equals new { e3.wmsno, wmsbllid = e3.bllid }
                      join e4 in WmsDc.wms_boci on new { dh = e3.lnkbocino, sndtmd = e3.lnkbocidat, e3.qu } equals new { e4.dh, e4.sndtmd, e4.qu }
                      join e5 in WmsDc.view_pssndgds on new { e4.dh, e4.clsid, e4.sndtmd, e.rcvdptid, e4.qu } equals new { e5.dh, e5.clsid, e5.sndtmd, e5.rcvdptid, e5.qu }
                      join e6 in WmsDc.dpt on e.rcvdptid equals e6.dptid
                      join e7 in WmsDc.wms_pkg on e2.gdsid equals e7.gdsid
                      where e.wmsno == wmsno
                      && e1.gdsid == gdsid
                      && (e.savdptid == LoginInfo.DefSavdptid || e.savdptid == LoginInfo.DefCsSavdptid)
                      //&& dpts.Contains(e.dptid.Trim())
                      && e.bllid == WMSConst.BLL_TYPE_DISPATCH
                      && e1.bzflg == 'n' && e1.qty>0    //大于0的商品
                      orderby e1.bzflg, e5.busid.Trim().Substring(e5.busid.Trim().Length - 1, 1), e5.busid.Trim().Substring(0, 3),
                      Convert.ToInt32(e5.rcvdptid), e2.gdsid, e1.qty
                      select e.rcvdptid;
            String shouldRcvdptid = qry.Distinct().FirstOrDefault();

            return shouldRcvdptid;
        }

        private bool CanBozByQty(string wmsno, string stkouno, string rcvdptid, string gdsid, double qty, int? rcdidx)
        {
            //得到该商品已经拣货的商品数量
            var qryRtv = (from e in WmsDc.wms_cang
                      join e1 in WmsDc.wms_cangdtl on new { e.wmsno, e.bllid } equals new { e1.wmsno, e1.bllid }
                      where e.bllid == WMSConst.BLL_TYPE_RETRIEVE
                      && e1.bokflg == 'y' && e1.tpcode == "y"
                      && e.wmsno == wmsno.Trim()
                      && e1.gdsid == gdsid.Trim()
                      select e1).ToArray();
            //如果拣货为0，就表示没有拣货
            if (qryRtv.Length == 0)
            {
                return false;
            }
            double dSumRtv = qryRtv.Sum(e => e.qty);

            //得到该商品应播种的数量
            var qryBz = (from e in WmsDc.stkot
                         join e1 in WmsDc.stkotdtl on e.stkouno equals e1.stkouno
                         where e.wmsno==wmsno && e.wmsbllid==WMSConst.BLL_TYPE_RETRIEVE
                         && e1.gdsid==gdsid
                         select e1
                             ).ToArray();
            //如果播种数量为0，就表示没有播种单
            if (qryBz.Length == 0)
            {
                return false;
            }

            //得到已经播了的数量
            double dHasBzQty = qryBz.Where(e => e.bzflg == 'y').Sum(e => e.qty);
            //得到拣货未播数量
            double dUnBzQty = dSumRtv - dHasBzQty;
            //如果拣货未播数量小于本次播种数量
            if (dUnBzQty < qty)
            {
                return false;
            }

            return true;
        }

        private void CkBzFlg(stkot p)
        {
            i(p.wmsno, "", System.DateTime.Now.ToString("yyyyMMddHHmmss.fff"), Request["rnd"], "3", LoginInfo.DefSavdptid);

            //盘点是否有为空的明细
            var qrydtl = p.stkotdtl.Where(e => e.qty == 0 && e.bzflg == GetN());
            foreach (stkotdtl d in qrydtl)
            {
                d.bzflg = GetY();
                d.bzr = LoginInfo.Usrid;
                d.bzdat = GetCurrentDate();
            }
            WmsDc.SubmitChanges();

            //修改播种标记
            p.bzflg = GetY();
            //审核配送单
            p.chkflg = GetY();
            p.chkdat = GetCurrentDay();
            p.ckr = LoginInfo.Usrid;

            //写入dtrlog
            //查看是否dtrlog已经有单据,没有就插入
            var qry = WmsDc.dtrlog
                        .Where(e => e.rcvdptid == p.rcvdptid && e.bllno == p.stkouno && e.bllid == p.bllid)
                        .Select(e => e.bllno);
            var arrqry = qry.ToArray();
            if (arrqry.Length <= 0)
            {
                dtrlog dl = new dtrlog();
                dl.bllid = p.bllid;
                dl.bllno = p.stkouno;
                dl.rcvdptid = p.rcvdptid;
                WmsDc.dtrlog.InsertOnSubmit(dl);
            }
            if (!(WmsDc.stklst.Where(e => e.stkouno == p.stkouno)).Any())
            {
                stklst astklst = new stklst();
                astklst.stkouno = p.stkouno;
                WmsDc.stklst.InsertOnSubmit(astklst);
                WmsDc.SubmitChanges();
            }

            i(p.wmsno, "", System.DateTime.Now.ToString("yyyyMMddHHmmss.fff"), Request["rnd"], "4", LoginInfo.DefSavdptid);
        }

        /// <summary>
        /// 播种单摘要信息
        /// </summary>
        /// <param name="wmsno"></param>
        /// <returns></returns>
        [PWR(Pwrid= WMSConst.WMS_BACK_播种查询, pwrdes = "播种查询")]
        public ActionResult GetBoZBllSummary(String wmsno)
        {
            String Dat = GetCurrentDay();            

            var qry = from e in WmsDc.stkot
                      join e1 in WmsDc.stkotdtl on e.stkouno equals e1.stkouno
                      join e2 in WmsDc.gds on e1.gdsid equals e2.gdsid
                      where e.wmsno == wmsno
                      && (e.savdptid == LoginInfo.DefSavdptid || e.savdptid == LoginInfo.DefCsSavdptid)
                      && dpts.Contains(e.dptid.Trim())
                      && e.bllid == WMSConst.BLL_TYPE_DISPATCH
                      && e.wmsbllid == WMSConst.BLL_TYPE_RETRIEVE
                      && Math.Round(e1.qty,2, MidpointRounding.AwayFromZero) != 0
                      select new { e, e1, e2 };
            var arrqry = qry.ToArray();
            var qrygrpall = from e in arrqry
                            group e by new { e.e.wmsno, e.e1.gdsid } into g
                            select new { g.Key.wmsno, g.Key.gdsid, ssum = g.Sum(e=>e.e1.preqty!=null?e.e1.preqty:e.e1.qty) };
            var arrqrygrpall = qrygrpall.ToArray();
            var qrygrpunbk = from e in arrqry
                             where (e.e1.bzflg == GetN() || e.e1.bzflg == null)
                             group e by new { e.e.wmsno, e.e1.gdsid } into g
                             select new { g.Key.wmsno, g.Key.gdsid, ssum = g.Sum(e => e.e1.qty) };
            var arrqrygrpunbk = qrygrpunbk.ToArray();
            var qrygrp = from e in arrqrygrpall
                         join e1 in arrqrygrpunbk on new { e.wmsno, e.gdsid } equals new { e1.wmsno, e1.gdsid }
                         into joinDefunbk
                         from e2 in joinDefunbk.DefaultIfEmpty()
                         group new { e, e2 } by e.wmsno into g
                         select new
                         {
                             wmsno = g.Key,
                             bkcount = g.Count(),
                             unbkcount = g.Count(a => a.e2 != null && a.e2.gdsid != null)
                         };

            /*
            var qry = from e in WmsDc.stkot
                      join e1 in WmsDc.stkotdtl on e.stkouno equals e1.stkouno                      
                      where e.wmsno == wmsno
                      && (e.savdptid == LoginInfo.DefSavdptid || e.savdptid == LoginInfo.DefCsSavdptid)
                      && dpts.Contains(e.dptid.Trim())
                      && e.bllid == WMSConst.BLL_TYPE_DISPATCH
                        && e.wmsbllid == WMSConst.BLL_TYPE_RETRIEVE
                      && Math.Round(e1.qty, 2, MidpointRounding.AwayFromZero) != 0
                      group new { e, e1 } by new { e.wmsno } into g
                      select new
                      {
                          g.Key,
                          bkcount = g.Count(),
                          unbkcount = g.Count(a => a.e1.bzflg == GetN() || a.e1.bzflg == null)
                      };
             */
            var wmsno1 = qrygrp.ToArray();
            if (wmsno1.Length <= 0)
            {
                return RNoData("N0050");
            }

            return RSucc("成功!", wmsno1, "S0039");
        }

        /// <summary>
        /// 选择未播种的20条记录
        /// </summary>
        /// <param name="wmsno"></param>
        /// <returns></returns>
        [PWR(Pwrid = WMSConst.WMS_BACK_播种查询, pwrdes = "播种查询")]
        public ActionResult GetBoZBllTop20List(String wmsno, string gdsid, int? pageid, int? pagesize)
        {
            pageid = pageid == null ? 1 : pageid;
            pagesize = pagesize == null ? 20 : pagesize;

            String Dat = GetCurrentDay();
   
            var qry = from e in WmsDc.stkot
                      join e1 in WmsDc.stkotdtl on e.stkouno equals e1.stkouno
                      join e2 in WmsDc.gds on e1.gdsid equals e2.gdsid
                      join e3 in WmsDc.wms_cang on new { e.wmsno, e.wmsbllid } equals new { e3.wmsno, wmsbllid = e3.bllid }
                      join ee4 in WmsDc.wms_boci on new { dh = e3.lnkbocino, sndtmd = e3.lnkbocidat, qu = e3.qu } equals new { ee4.dh, ee4.sndtmd, ee4.qu }
                      into joinBoci
                      from e4 in joinBoci.DefaultIfEmpty()
                      join ee5 in WmsDc.view_pssndgds on new { e4.dh, e4.clsid, e4.sndtmd, e.rcvdptid, e4.qu } equals new { ee5.dh, ee5.clsid, ee5.sndtmd, ee5.rcvdptid, ee5.qu }
                      into joinPssndgds
                      from e5 in joinPssndgds.DefaultIfEmpty()
                      join e6 in WmsDc.dpt on e.rcvdptid equals e6.dptid
                      where e.wmsno == wmsno
                      && (e.savdptid == LoginInfo.DefSavdptid || e.savdptid == LoginInfo.DefCsSavdptid)
                      && dpts.Contains(e.dptid.Trim())
                      && e.bllid == WMSConst.BLL_TYPE_DISPATCH
                      && e.wmsbllid == WMSConst.BLL_TYPE_RETRIEVE
                      && (e1.bzflg == GetN() || e1.bzflg == null)
                      orderby e1.gdsid
                      group e1 by new { e.wmsno, e.wmsbllid, e1.gdsid, e2.gdsdes, e2.spc, e2.bsepkg } into g
                      select new
                      {
                          g.Key.wmsno,
                          g.Key.wmsbllid,
                          g.Key.gdsid,
                          g.Key.gdsdes,
                          g.Key.spc,
                          g.Key.bsepkg,
                          sqty = g.Sum(eqty => eqty.qty),
                          sqtypre = g.Sum(eqty => eqty.preqty == null ? eqty.qty : eqty.preqty)
                      };
            var q = from e2 in qry
                    join e3 in WmsDc.wms_pkg on new { e2.gdsid } equals new { e3.gdsid }
                    into joinPkg
                    from e4 in joinPkg.DefaultIfEmpty()
                    select new
                    {
                        e2.wmsno,
                        e2.gdsid,
                        e2.gdsdes,
                        e2.spc,
                        e2.bsepkg,
                        e2.sqty,
                        e2.sqtypre,
                        e4.cnvrto,
                        pkgdes = e4.pkgdes.Trim(),
                        pkg03 = GetPkgStr(e2.sqty, e4.cnvrto, e4.pkgdes),
                        pkg03pre = GetPkgStr(e2.sqty, e4.cnvrto, e4.pkgdes)
                    };
            var wmsno1 = q.Take(20).ToArray();
            if (wmsno1.Length <= 0)
            {
                return RNoData("N0051");
            }

            return RSucc("成功！", wmsno1, "S0040");
        }

        /// <summary>
        /// 选择未播种的20条记录
        /// </summary>
        /// <param name="wmsno"></param>
        /// <returns></returns>
        [PWR(Pwrid = WMSConst.WMS_BACK_播种查询, pwrdes = "播种查询")]
        public ActionResult GetBoZBllTop20(String wmsno)
        {

            String Dat = GetCurrentDay();

            var qry = from e in WmsDc.stkot
                      join e1 in WmsDc.stkotdtl on e.stkouno equals e1.stkouno
                      join e2 in WmsDc.gds on e1.gdsid equals e2.gdsid
                      join e3 in WmsDc.wms_cang on new { e.wmsno, e.wmsbllid } equals new { e3.wmsno, wmsbllid = e3.bllid }
                      join ee4 in WmsDc.wms_boci on new { dh = e3.lnkbocino, sndtmd = e3.lnkbocidat, qu=e3.qu } equals new { ee4.dh, ee4.sndtmd ,ee4.qu}
                      into joinBoci
                      from e4 in joinBoci.DefaultIfEmpty()
                      join ee5 in WmsDc.view_pssndgds on new { e4.dh, e4.clsid, e4.sndtmd, e.rcvdptid, e4.qu } equals new { ee5.dh, ee5.clsid, ee5.sndtmd, ee5.rcvdptid, ee5.qu }
                      into joinPssndgds
                      from e5 in joinPssndgds.DefaultIfEmpty()
                      join e6 in WmsDc.dpt on e.rcvdptid equals e6.dptid
                      where e.wmsno == wmsno
                      && (e.savdptid == LoginInfo.DefSavdptid || e.savdptid == LoginInfo.DefCsSavdptid)
                      && dpts.Contains(e.dptid.Trim())
                      && e.bllid == WMSConst.BLL_TYPE_DISPATCH
                      && e.wmsbllid == WMSConst.BLL_TYPE_RETRIEVE
                      && (e1.bzflg == GetN() || e1.bzflg == null)
                      orderby e1.gdsid
                      group e1 by new { e.wmsno, e.wmsbllid, e1.gdsid, e2.gdsdes, e2.spc, e2.bsepkg } into g
                      select new
                      {
                          g.Key.wmsno,
                          g.Key.wmsbllid,
                          g.Key.gdsid,
                          g.Key.gdsdes,
                          g.Key.spc,
                          g.Key.bsepkg,
                          sqty = g.Sum(eqty => Math.Round(eqty.qty, 2, MidpointRounding.AwayFromZero)),
                          sqtypre = g.Sum(eqty => Math.Round(eqty.preqty == null ? eqty.qty : eqty.preqty.Value, 2, MidpointRounding.AwayFromZero))
                      };
            qry = qry.Where(e => e.sqtypre > 0);
            var q = from e2 in qry
                    join e3 in WmsDc.wms_pkg on new { e2.gdsid  } equals new { e3.gdsid  }
                    into joinPkg
                    from e4 in joinPkg.DefaultIfEmpty()
                    select new
                    {
                        e2.wmsno,
                        e2.gdsid,
                        e2.gdsdes,
                        e2.spc,
                        e2.bsepkg,
                        e2.sqty,
                        e2.sqtypre,
                        bzedall = (from e in WmsDc.stkot
                                   join e1 in WmsDc.stkotdtl on e.stkouno equals e1.stkouno
                                   where e.wmsno == e2.wmsno && e.wmsbllid == e2.wmsbllid && e1.gdsid == e2.gdsid
                                   && e.bzflg == GetN()
                                   group e1 by new { e.wmsno, e.wmsbllid } into g1
                                   select g1).Count() == 0 ? GetY() : GetN(),
                        e4.cnvrto,
                        pkgdes = e4.pkgdes.Trim(),
                        pkg03 = GetPkgStr(e2.sqty, e4.cnvrto, e4.pkgdes),
                        pkg03pre = GetPkgStr(e2.sqtypre, e4.cnvrto, e4.pkgdes)
                    };
            var wmsno1 = q.Take(20).ToArray();
            if (wmsno1.Length <= 0)
            {
                return RNoData("N0052");
                //return RInfo( "I0085" );
            }

            return RSucc("成功！", wmsno1, "S0041");
        }

        /// <summary>
        /// 根据商品条码和拣货单号查找播种单
        /// </summary>
        /// <param name="gdsid">条码/货号</param>
        /// <returns></returns>
        [PWR(Pwrid = WMSConst.WMS_BACK_播种查询, pwrdes = "播种查询")]
        public ActionResult GetBoZBllByGdsid(String wmsno, String gdsid)
        {
            gdsid = GetGdsidByGdsidOrBcd(gdsid);
            if (gdsid == null)
            {
                return RInfo( "I0086" );
            }

            String Dat = GetCurrentDay();

            var qry = from e in WmsDc.stkot
                      join e1 in WmsDc.stkotdtl on e.stkouno equals e1.stkouno
                      join e2 in WmsDc.gds on e1.gdsid equals e2.gdsid
                      join e3 in WmsDc.wms_cang on new { e.wmsno, e.wmsbllid } equals new { e3.wmsno, wmsbllid = e3.bllid }
                      join ee4 in WmsDc.wms_boci on new { dh = e3.lnkbocino, sndtmd = e3.lnkbocidat, qu=e3.qu } equals new { ee4.dh, ee4.sndtmd ,ee4.qu}
                      into joinBoci from e4 in joinBoci.DefaultIfEmpty()
                      join ee5 in WmsDc.view_pssndgds on new { e4.dh, e4.clsid, e4.sndtmd, e.rcvdptid, e4.qu } equals new { ee5.dh, ee5.clsid, ee5.sndtmd, ee5.rcvdptid, ee5.qu }
                      into joinPssndgds from e5 in joinPssndgds.DefaultIfEmpty()
                      join e6 in WmsDc.dpt on e.rcvdptid equals e6.dptid
                      join e7 in WmsDc.dpt on e2.dptid equals e7.dptid
                      where e.wmsno == wmsno
                      && e1.gdsid == gdsid
                      && (e.savdptid == LoginInfo.DefSavdptid || e.savdptid == LoginInfo.DefCsSavdptid)
                      && dpts.Contains(e.dptid.Trim())
                      && e.bllid == WMSConst.BLL_TYPE_DISPATCH
                      && e.wmsbllid == WMSConst.BLL_TYPE_RETRIEVE
                      //&& (e.bzflg == GetN() || e.bzflg == null)                      
                      select new
                      {
                          e.stkouno,
                          e.rcvdptid,
                          e.dptid,
                          dptdes1 = e7.dptdes,
                          e.savdptid,
                          e.mkedat,
                          e1.gdsid,
                          e1.rcdidx,
                          e2.gdsdes,
                          e2.spc,
                          e2.bsepkg,
                          e1.pkgqty,
                          qty = Math.Round(e1.qty, 4, MidpointRounding.AwayFromZero),
                          e5.busid,
                          e6.dptdes,
                          e1.bzflg,
                          e1.bzdat,
                          e1.bzr,
                          preqty = e1.preqty == null ? e1.qty : e1.preqty
                      };
            qry = qry.Where(e => e.qty > 0);
            var q = from e2 in qry
                    join e3 in
                        (from m in WmsDc.wms_pkg group m by new { m.gdsid, m.cnvrto, m.pkgdes } into g select g.Key)
                    on new { e2.gdsid } equals new { e3.gdsid }
                    into joinPkg
                    from e4 in joinPkg.DefaultIfEmpty()
                    orderby e2.bzflg, e2.busid.Trim().Substring(e2.busid.Trim().Length - 1, 1), e2.busid.Trim().Substring(0, 3),
                      Convert.ToInt32(e2.rcvdptid), e2.gdsid, e2.qty
                    select new
                    {
                        e2.bsepkg,
                        e2.busid,
                        e2.bzdat,
                        e2.bzflg,
                        e2.bzr,
                        e2.dptdes,
                        e2.dptid,
                        e2.dptdes1,
                        e2.gdsdes,
                        e2.gdsid,
                        e2.mkedat,
                        e2.pkgqty,
                        e2.preqty,
                        e2.qty,
                        e2.rcdidx,
                        e2.rcvdptid,
                        e2.savdptid,
                        e2.spc,
                        e2.stkouno,
                        e4.cnvrto,
                        e4.pkgdes,
                        pkg03 = GetPkgStr(e2.qty, e4.cnvrto, e4.pkgdes),
                        pkg03pre = GetPkgStr(e2.preqty, e4.cnvrto, e4.pkgdes)
                    };
            var wmsno1 = q.ToArray();
            if (wmsno1.Length <= 0)
            {
                return RNoData("N0053");
            }

            var extObj = wmsno1.GroupBy(e => new { e.dptid, e.dptdes1, e.gdsid, e.gdsdes, e.cnvrto, e.pkgdes })
                    .Select(ek => new
                    {
                        sqty = ek.Sum(e1 => e1.qty),
                        ek.Key.gdsid,
                        ek.Key.gdsdes,
                        ek.Key.cnvrto,
                        ek.Key.dptid,
                        ek.Key.dptdes1,
                        pkgdes = ek.Key.pkgdes.Trim(),
                        pkg03 = GetPkgStr(ek.Sum(e1 => e1.qty), ek.Key.cnvrto, ek.Key.pkgdes),
                        pkg03pre = GetPkgStr(ek.Sum(e1 => e1.preqty), ek.Key.cnvrto, ek.Key.pkgdes),
                    });

            return RSucc("成功", wmsno1, extObj, "S0042");
        }

        /// <summary>
        /// 根据日程查询播种批次单据
        /// </summary>        
        /// <returns></returns>
        [PWR(Pwrid = WMSConst.WMS_BACK_播种查询, pwrdes = "播种查询")]
        public ActionResult GetCurrBoZBll()
        {


            String Fscprdid = GetCurrentFscprdid();     //得到会计期间
            var qry = from e in WmsDc.stkot
                      join e1 in WmsDc.wms_cang on new { e.wmsno, bllid = e.wmsbllid } equals new { e1.wmsno, e1.bllid }
                      where e.stkouno.StartsWith(Fscprdid)
                      //e.mkedat.Substring(0, 8) == GetCurrentDay()
                      && e.bzflg == GetN() && e.zdflg == GetY() && e1.chkflg == GetY()
                      && e.wmsno != null && (e.savdptid == LoginInfo.DefSavdptid || e.savdptid == LoginInfo.DefCsSavdptid)
                      && e.wmsbllid == WMSConst.BLL_TYPE_RETRIEVE
                      //&& dpts.Contains(e.dptid.Trim())
                          //&& WmsDc.dpt.Where(ed=>ed.dptid==e.dptid).Any()                      
                      //&& LoginInfo.DatPwrs.Where(et=>et.dptid==e.dptid).Any()
                      && e.bllid == WMSConst.BLL_TYPE_DISPATCH
                      && e.wmsbllid == WMSConst.BLL_TYPE_RETRIEVE
                      && !(from ebz in WmsDc.wms_bzcnv
                          where ebz.sndtmd.Substring(2, 4) == Fscprdid && ebz.savdptid == LoginInfo.DefSavdptid
                              && ebz.wmsno == e.wmsno && ebz.wmsbllid == e.wmsbllid
                          select ebz).Any()
                      group new { e.wmsno, e.wmsbllid, e.zddat, e1.qu,e.dptid } by new { e.wmsno, e.wmsbllid, e.zddat, e1.qu, e.dptid } into g
                      select new
                      {
                          g.Key.wmsno,
                          g.Key.wmsbllid,
                          g.Key.zddat,
                          g.Key.qu,
                          g.Key.dptid
                      };

            var arrqrymst = qry.ToArray();
            var arrqrymst1 = arrqrymst.Where(e => IsExistsPwrByDptidAndQu(e.dptid, e.qu).Any())
                        .GroupBy(e => new { e.wmsno, e.wmsbllid, e.zddat, e.qu })
                        .Select(e => e.Key)
                        .ToArray();
            if (arrqrymst1.Length <= 0)
            {
                return RNoData("N0054");
            }

            return RSucc("成功", arrqrymst1, "S0043");
        }

        /// <summary>
        /// 得到波次的列表
        /// </summary>
        /// <param name="wmsno">播种单号</param>
        /// <returns></returns>
        /*public ActionResult GetBoZDtls(String wmsno)
        {

            var qry = from e in WmsDc.wms_bzmst
                      join e1 in WmsDc.wms_bzdtl on e.wmsno equals e1.wmsno
                      join e2 in WmsDc.gds on e1.gdsid equals e2.gdsid
                      where e.savdptid == LoginInfo.DefSavdptid
                      && dpts.Contains(e2.dptid)
                      && e.wmsno == wmsno
                      orderby e2.dptid, e1.gdsid
                      select e1;
            var arrqry = qry.ToArray();
            if (arrqry.Length <= 0)
            {
                return RInfo( "I0087" );
            }
            return RSucc("成功", arrqry, "S0044");
        }*/


        /// <summary>
        /// 播种查询
        /// </summary>
        /// <param name="bllid">播种单据类型(206配送、501外销、内调112)</param>
        /// <param name="dat">发货日期</param>
        /// <param name="boci">播种波次</param>
        /// <param name="gdsid">商品货号</param>
        /// <param name="rcvdptid">收货部门</param>
        /// <param name="busid">车次号</param>
        /// <returns></returns>
        [PWR(Pwrid = WMSConst.WMS_BACK_播种查询, pwrdes = "播种查询")]
        public ActionResult FindBll(String bllid, String dat, String boci, String gdsid, String rcvdptid, String busid)
        {
            if (string.IsNullOrEmpty(dat))
            {
                return RInfo( "I0088" );
            }
            var arrqrymst = FindBllFromCangMst107(bllid, dat, boci, gdsid, rcvdptid, busid);
            if (arrqrymst.Length <= 0)
            {
                return RNoData("N0055");
            }
            return RSucc("成功", arrqrymst, "S0045");
        }
    }
}
