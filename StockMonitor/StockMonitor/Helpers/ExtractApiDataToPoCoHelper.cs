using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockMonitor.Models.JSONModels;
using StockMonitor.Models.POCO;

namespace StockMonitor.Helpers
{
    public static class ExtractApiDataToPoCoHelper
    {
        public static CompanyDataRow GetCompanyDataRow(string symbol)
        {
            FmgCompanyProfile fmgCompanyProfile = RetrieveJsonDataHelper.RetrieveFmgCompanyProfile(symbol);
            FinnQuote finnQuote = RetrieveJsonDataHelper.RetrieveFinnQuote(symbol);
            FinnCompanyProfile finnCompanyProfile = RetrieveJsonDataHelper.RetrieveFinnCompanyProfile(symbol);
            CompanyDataRow companyDataRow = new CompanyDataRow();
            companyDataRow.Symbol = symbol;
            companyDataRow.Price = finnQuote.C;
            companyDataRow.ChangePercentage = (finnQuote.C - finnQuote.O) / finnQuote.O * 100;
            companyDataRow.Change = finnQuote.C - finnQuote.O;
            companyDataRow.MarketCapital = fmgCompanyProfile.MktCap;
            companyDataRow.Sector = fmgCompanyProfile.Sector;
            companyDataRow.EmployeesTotal = finnCompanyProfile.EmployeeTotal;

            return companyDataRow;
        }

    }
}
