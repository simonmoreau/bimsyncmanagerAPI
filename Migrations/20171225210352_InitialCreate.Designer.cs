﻿// <auto-generated />
using bimsyncManagerAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using System;

namespace bimsyncManagerAPI.Migrations
{
    [DbContext(typeof(UserContext))]
    [Migration("20171225210352_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125");

            modelBuilder.Entity("bimsyncManagerAPI.Models.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AccessToken");

                    b.Property<string>("Name");

                    b.Property<string>("PowerBiSecret");

                    b.Property<string>("RefreshDate");

                    b.Property<string>("RefreshToken");

                    b.Property<string>("TokenExpireIn");

                    b.Property<string>("TokenType");

                    b.Property<string>("bimsync_id");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });
#pragma warning restore 612, 618
        }
    }
}
