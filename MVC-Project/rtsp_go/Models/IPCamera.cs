﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace rtsp_go.Models
{
    public class IPCamera
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string IP { get; set; }
        [Required]
        public string Port { get; set; }
    }
}