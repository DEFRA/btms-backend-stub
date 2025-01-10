using Microsoft.AspNetCore.Builder;

namespace BtmsBackendStub.Test.Config;

public class EnvironmentTest
{

   [Fact]
   public void IsNotDevModeByDefault()
   {
      var _builder = WebApplication.CreateBuilder();

      var isDev = BtmsBackendStub.Config.Environment.IsDevMode(_builder);

      Assert.False(isDev);
   }
}
