using System;
using Bugfree.OracleHospitality.Clients.PosParselets;

namespace Bugfree.OracleHospitality.Clients;

public class PointOfSaleOperations
{
    public int TerminalIdLowerBound { get; set; }
    public int TerminalIdUpperBound { get; set; }

    public void Validate()
    {
        if (TerminalIdLowerBound < TerminalId.MinValue)
            throw new ArgumentException(nameof(TerminalIdLowerBound));
        if (TerminalIdUpperBound > TerminalId.MaxValue)
            throw new ArgumentException(nameof(TerminalIdLowerBound));
        if (TerminalIdLowerBound >= TerminalIdUpperBound)
            throw new ArgumentException($"{nameof(TerminalIdLowerBound)} must be less than {nameof(TerminalIdUpperBound)}");
    }
}

public class CustomerRelationshipManagementOperations
{
    public string RequestSourceName { get; set; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(RequestSourceName))
            throw new ArgumentException(nameof(RequestSourceName));
    }
}

public class OracleHospitalityClientsOptions
{
    public Uri StoredValueServiceUrl { get; set; }
    public string LogonName { get; set; }
    public string Password { get; set; }
    public PointOfSaleOperations PointOfSaleOperations { get; set; }
    public CustomerRelationshipManagementOperations CustomerRelationshipManagementOperations { get; set; }

    public void Validate()
    {
        if (StoredValueServiceUrl == null)
            throw new ArgumentNullException(nameof(StoredValueServiceUrl));
        if (string.IsNullOrWhiteSpace(LogonName) || LogonName.Length > 16)
            throw new ArgumentException(nameof(LogonName));
        if (string.IsNullOrWhiteSpace(Password) || Password.Length > 16)
            throw new ArgumentException(nameof(Password));

        if (PointOfSaleOperations == null)
            throw new ArgumentNullException(nameof(PointOfSaleOperations));
        PointOfSaleOperations.Validate();

        if (CustomerRelationshipManagementOperations == null)
            throw new ArgumentNullException(nameof(CustomerRelationshipManagementOperations));
        CustomerRelationshipManagementOperations.Validate();
    }
}