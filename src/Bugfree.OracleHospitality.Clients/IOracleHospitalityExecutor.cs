using System.Threading.Tasks;
using System.Threading;
using XE = System.Xml.Linq.XElement;

namespace Bugfree.OracleHospitality.Clients
{
    public interface IOracleHospitalityExecutor
    {
        Task<XE> ExecuteAsync(XE in0, CancellationToken cancellationToken);
    }
}