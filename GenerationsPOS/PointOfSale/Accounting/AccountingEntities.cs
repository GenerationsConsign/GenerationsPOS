namespace GenerationsPOS.PointOfSale.Accounting
{
    public record struct QBObject(string Id, string Name);

    public record struct Consignor(QBObject QB);

    public record struct Customer(QBObject QB);

    public record struct CompanyAccounts(string PurchasesAccount, string AssetAccount, string IncomeAccount);
}
