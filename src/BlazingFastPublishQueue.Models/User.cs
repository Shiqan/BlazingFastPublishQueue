using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazingFastPublishQueue.Models
{
    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public CompletionField Suggest { get; set; }
    }
}
