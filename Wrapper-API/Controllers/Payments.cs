﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Wallet_Wrapper;
using Wallet_Wrapper.Objects;

namespace Wrapper_API.Controllers
{
    public class Payments : Controller
    {
        [HttpGet("Balance")]
        public async Task<float> Balance()
        {
            return (await Cli_Gets.GetWalletInfo()).balance;
        }

        [HttpGet("PayInAddress")]
        public async Task<string> PayIn()
        {
            WUser u = Authentication.CheckAuthed(Request.Cookies["authkey"]);
            if (u == null)
            {
                Response.StatusCode = 401;
                return "";
            }

            string addr = (await Cli_Gets.GetNewWalletAddress()).Trim();
            u.inAddress = addr;
            u.Updated();
            return addr;
        }

        [HttpGet("Confirm")]
        public async Task<object> ConfirmTransaction([FromQuery] string txId)
        {
            if (txId==null)
            {
                Response.StatusCode = 400;
                return false;
            }

            WUser u = Authentication.CheckAuthed(Request.Cookies["authkey"]);
            if (u == null)
            {
                Response.StatusCode = 401;
                return false;
            }

            Object a = await Cli_Payments.ConfirmPayment(u.inAddress, txId.Trim());
            if (a.GetType() == typeof(Account))
            {
                u.balance += ((Account)a).amount;
                u.Updated();
                return u;
            }
            Response.StatusCode = 400;
            return a;
        }

        [HttpGet("Withdraw")]
        public async Task<string> Withdraw([FromQuery] string outAddr, [FromQuery] float amount)
        {
            amount = Math.Abs(amount);

            WUser u = Authentication.CheckAuthed(Request.Cookies["authkey"]);
            if (u == null)
            {
                Response.StatusCode = 401;
                return "Not Signed In";
            }

            if (!(await Cli_Gets.VerifyAddress(outAddr)).isvalid)
            {
                Response.StatusCode = 400;
                return "Dest Address Is Invalid";
            }

            if (u.balance >= amount)
            {
                u.balance -= amount;
                u.Updated();

                Cli_Manager.ResponseBody<object> t = await Cli_Payments.PayOut(outAddr, amount);

                if (t.result!=null)  return t.result.ToString();
                else
                {
                    Response.StatusCode = 500;
                    return "A Server Error Occured";
                }
            }
            else
            {
                Response.StatusCode = 400;
                return "You dont have enough balance.";
            }
        }
    }
}