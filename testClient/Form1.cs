using System;
using System.Windows.Forms;
using testClient.SearchService;
using testClient.ServiceReference1;

namespace testClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var client = new Service1Client())
            {
                client.DoWork();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (var serarcClient = new SearchServiceClient())
            {
                var result = serarcClient.Search(GetSearch());
                MessageBox.Show(result.Metadata.Status.ToString());
            }
        }

        private static SearchRequest GetSearch()
        {
            var ssQuery = new SimpleQuery();
            ssQuery.SearchTypes =new[]{new FederationNode{ Value = "Contact"} };
            ssQuery.QueryString = "Jonas Syrstad";
            ssQuery.GetNavigators = true;
            ssQuery.DetailLevel = "99";
            var ssRequest = new SearchRequest
            {
                Query = ssQuery,
                Metadata = new RequestMetadata
                {
                    RequestId = Guid.NewGuid().ToString(),
                    ConfigSet = "1881.idefix.search.dev",
                    SystemSource = "test"
                }
            };
            return ssRequest;
        }


    }
}
