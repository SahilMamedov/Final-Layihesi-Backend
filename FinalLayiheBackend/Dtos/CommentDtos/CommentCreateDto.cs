﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalLayiheBackend.Dtos.CommentDtos
{
    public class CommentCreateDto
    {
        public string AppUserId { get; set; }
        public int ProductId { get; set; }
        public string Content { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
