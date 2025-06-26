﻿using System.ComponentModel.DataAnnotations.Schema;
using Craft.Domain;

namespace Craft.TestDataStore.Models;


public class Store : BaseEntity
{
    public string? City { get; set; }

    [ForeignKey("CompanyId")]
    public Company? Company { get; set; }
    public KeyType CompanyId { get; set; }

    public string? Name { get; set; }
}
