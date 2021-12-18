using System;

namespace Bugfree.OracleHospitality.Clients.UnitTests.Builders;

public class OracleHospitalityClientOptionsBuilder
{
    private Uri _storedValueServiceUrl = new("http://testUrl");
    private string _logonName = "testUser";
    private string _password = "testPassword";
    private int _terminalIdLowerBound;
    private int _terminalIdUpperBound = 1000;
    private string _requestSourceName = "acme.com";

    public OracleHospitalityClientOptionsBuilder WithUrl(Uri url)
    {
        _storedValueServiceUrl = url;
        return this;
    }

    public OracleHospitalityClientOptionsBuilder WithLogonName(string logonName)
    {
        _logonName = logonName;
        return this;
    }

    public OracleHospitalityClientOptionsBuilder WithPassword(string password)
    {
        _password = password;
        return this;
    }

    public OracleHospitalityClientOptionsBuilder WithTerminalIdLowerBound(int terminalIdLowerBound)
    {
        _terminalIdLowerBound = terminalIdLowerBound;
        return this;
    }

    public OracleHospitalityClientOptionsBuilder WithTerminalIdUpperBound(int terminalIdUpperBound)
    {
        _terminalIdUpperBound = terminalIdUpperBound;
        return this;
    }

    public OracleHospitalityClientOptionsBuilder WithRequestSourceName(string requestSourceName)
    {
        _requestSourceName = requestSourceName;
        return this;
    }

    public OracleHospitalityClientsOptions Build()
    {
        return new OracleHospitalityClientsOptions
        {
            StoredValueServiceUrl = _storedValueServiceUrl,
            LogonName = _logonName,
            Password = _password,
            PointOfSaleOperations = new PointOfSaleOperations
            {
                TerminalIdLowerBound = _terminalIdLowerBound,
                TerminalIdUpperBound = _terminalIdUpperBound
            },
            CustomerRelationshipManagementOperations = new CustomerRelationshipManagementOperations
            {
                RequestSourceName = _requestSourceName
            }
        };
    }

    public static implicit operator OracleHospitalityClientsOptions(OracleHospitalityClientOptionsBuilder builder)
    {
        return builder.Build();
    }
}