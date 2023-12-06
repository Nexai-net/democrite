// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.Configuration.UnitTests
{
    using Microsoft.Extensions.Configuration;

    using NFluent;

    using System.Text;

    using Xunit;

    public sealed class TemplatedConfigurationUnitTests
    {
        #region Fields

        private const string TestFullStringName = "Democrite.Framework.Toolbox.Configuration.UnitTests.SampleFullConfiguration.json";

        #endregion

        #region Nested

        public class DBConnectionTest
        {
            public string? ConnectionString { get; set; }

            public string? Database { get; set; }
        }

        public class ServiceTest
        {
            public ServiceTest(string baseUrl,
                               string apiPrivateKey,
                               ServiceConnectionTest? security = null)
            {
                this.BaseUrl = baseUrl;
                this.APIPrivateKey = apiPrivateKey;
                this.Security = security;
            }

            public string BaseUrl { get; }
            public string APIPrivateKey { get; }
            public ServiceConnectionTest? Security { get; }
        }

        public class ServiceConnectionTest
        {
            public ServiceConnectionTest(string baseUrl,
                                         string authenticationType,
                                         string login,
                                         string password,
                                         string secretAPIKey)
            {
                this.BaseUrl = baseUrl;
                this.AuthenticationType = authenticationType;
                this.Login = login;
                this.Password = password;
                this.SecretAPIKey = secretAPIKey;
            }

            public string BaseUrl { get; }
            public string AuthenticationType { get; }
            public string Login { get; }
            public string Password { get; }
            public string SecretAPIKey { get; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Test <see cref="TemplatedConfigurationProxyProvider"/> loading from source
        /// </summary>
        [Fact]
        public void TemplatedConfiguration_Load()
        {
            var cfg = CreateConfigurationRoot();

            var debugInfo = cfg.GetDebugView();
            Check.That(debugInfo).IsNotNull();
        }

        /// <summary>
        /// Test <see cref="TemplatedConfigurationProxyProvider"/> loading from source with no template defined
        /// </summary>
        [Fact]
        public void TemplatedConfiguration_NoTemplate()
        {
            var config = new ConfigurationBuilder();

            var jsoncfg = """
                {
                    "ConnectionStringConnectionString" : "toto",
                    "Database" : "totoDB"
                } 
                """;

            using (var fullStream = new MemoryStream(Encoding.Default.GetBytes(jsoncfg)))
            {
                Check.That(fullStream).IsNotNull();
                config.AddJsonStream(fullStream!);

                var cfg = config.ToTemplatedConfiguration()
                                .Build();

                Check.That(cfg).IsNotNull();

                var connection = cfg.GetValue<string>("ConnectionStringConnectionString");
                Check.That(connection).IsNotNull().And.IsEqualTo("toto");
            }
        }

        /// <summary>
        /// Test <see cref="TemplatedConfigurationProxyProvider"/> loading from source with templates defined
        /// </summary>
        [Fact]
        public void TemplatedConfiguration_MutipleTemplates()
        {
            var config = new ConfigurationBuilder();

            var jsoncfg = """
                {
                    "SharedTemplates":
                    {
                        "TplA" : {
                            "ConnectionString" : "tata",
                        }
                    },

                    "Internal" : {

                        "SharedTemplates":
                        {
                            "TplB" : {
                                "ConnectionString" : "toto",
                            }
                        },
                    },

                    "ConnectionA": {
                        "Template" : "TplA"
                    },

                    "ConnectionB": {
                        "Template" : "TplB"
                    },
                } 
                """;

            using (var fullStream = new MemoryStream(Encoding.Default.GetBytes(jsoncfg)))
            {
                Check.That(fullStream).IsNotNull();
                config.AddJsonStream(fullStream!);

                var cfg = config.ToTemplatedConfiguration()
                                .Build();

                Check.That(cfg).IsNotNull();

                var connectionA = new DBConnectionTest();
                cfg.Bind("ConnectionA", connectionA);
                Check.That(connectionA.ConnectionString).IsNotNull().And.IsEqualTo("tata");

                var connectionB = new DBConnectionTest();
                cfg.Bind("ConnectionB", connectionB);
                Check.That(connectionB.ConnectionString).IsNotNull().And.IsEqualTo("toto");
            }
        }

        /// <summary>
        /// Test <see cref="TemplatedConfigurationProxyProvider"/> loading from source and apply template
        /// </summary>
        [Fact]
        public void TemplatedConfiguration_ApplyTemplate()
        {
            var cfg = CreateConfigurationRoot();

            var dbInfo = new DBConnectionTest();
            cfg.Bind("Democrite:Storages:Memberships", dbInfo);

            Check.That(dbInfo.Database).IsNotNull().And.IsEqualTo("MemberTableDebug");
            Check.That(dbInfo.ConnectionString).IsNotNull().And.IsEqualTo("mongod://volatile");

            var dbAnalysisResultInfo = new DBConnectionTest();
            cfg.Bind("Democrite:Storages:AnalysisResult", dbAnalysisResultInfo);

            Check.That(dbAnalysisResultInfo.Database).IsNotNull().And.IsEqualTo("Analysis");
            Check.That(dbAnalysisResultInfo.ConnectionString).IsNotNull().And.IsEqualTo("mongod://persistence");
        }

        /// <summary>
        /// Test <see cref="TemplatedConfigurationProxyProvider"/> loading from source and apply template from ctor injection
        /// </summary>
        [Fact]
        public void TemplatedConfiguration_ApplyTemplate_BindCtor()
        {
            var cfg = CreateConfigurationRoot();

            var serviceInfo = cfg.GetSection("Democrite:Services:GoogleTraductor").Get<ServiceTest>();

            Check.That(serviceInfo).IsNotNull();
            Check.That(serviceInfo!.BaseUrl).IsNotNull().And.IsEqualTo("https://api.google.com/translate");
            Check.That(serviceInfo!.APIPrivateKey).IsNotNull().And.IsEqualTo("qsdqsdqsd-qsdqsd-qsdqsdqsdqs-qsdqsdqsdq");

            var security = serviceInfo.Security;

            Check.That(security).IsNotNull();
            Check.That(security!.BaseUrl).IsNotNull().And.IsEqualTo("https://oauth.google.com");
            Check.That(security.AuthenticationType).IsNotNull().And.IsEqualTo("Credentials");
            Check.That(security.Login).IsNotNull().And.IsEqualTo("LOGIN");
            Check.That(security.Password).IsNotNull().And.IsEqualTo("PASS");
            Check.That(security.SecretAPIKey).IsNotNull().And.IsEqualTo("zeazeadazaz-azeaze-azeazeaze-azeazeazeaea");
        }

        /// <summary>
        /// Create and init <see cref="IConfigurationRoot"/> from embeded json config
        /// </summary>
        private static IConfigurationRoot CreateConfigurationRoot()
        {
            var config = new ConfigurationBuilder();

            using (var fullStream = typeof(TemplatedConfigurationUnitTests).Assembly.GetManifestResourceStream(TestFullStringName))
            {
                Check.That(fullStream).IsNotNull();
                config.AddJsonStream(fullStream!);

                var cfg = config.ToTemplatedConfiguration()
                                .Build();

                Check.That(cfg).IsNotNull();

                return cfg!;
            }
        }

        #endregion
    }
}
