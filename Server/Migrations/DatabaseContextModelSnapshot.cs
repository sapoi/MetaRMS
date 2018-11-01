﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Server;
using System;

namespace Server.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.2-rtm-10011");

            modelBuilder.Entity("SharedLibrary.Models.ApplicationModel", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("ApplicationDescriptorJSON")
                        .IsRequired()
                        .HasColumnName("descriptor");

                    b.Property<string>("LoginApplicationName")
                        .IsRequired()
                        .HasColumnName("login_application_name");

                    b.HasKey("Id");

                    b.ToTable("applications");
                });

            modelBuilder.Entity("SharedLibrary.Models.DataModel", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<long>("ApplicationId")
                        .HasColumnName("application_id");

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasColumnName("data");

                    b.Property<long>("DatasetId")
                        .HasColumnName("dataset_id");

                    b.HasKey("Id");

                    b.HasIndex("ApplicationId");

                    b.ToTable("data");
                });

            modelBuilder.Entity("SharedLibrary.Models.RightsModel", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<long>("ApplicationId")
                        .HasColumnName("application_id");

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasColumnName("data");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name");

                    b.HasKey("Id");

                    b.HasIndex("ApplicationId");

                    b.ToTable("rights");
                });

            modelBuilder.Entity("SharedLibrary.Models.UserModel", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<long>("ApplicationId")
                        .HasColumnName("application_id");

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasColumnName("data");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnName("password");

                    b.Property<long>("RightsId")
                        .HasColumnName("rights_id");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnName("username");

                    b.HasKey("Id");

                    b.HasIndex("ApplicationId");

                    b.HasIndex("RightsId");

                    b.ToTable("users");
                });

            modelBuilder.Entity("SharedLibrary.Models.DataModel", b =>
                {
                    b.HasOne("SharedLibrary.Models.ApplicationModel", "Application")
                        .WithMany("Datas")
                        .HasForeignKey("ApplicationId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("SharedLibrary.Models.RightsModel", b =>
                {
                    b.HasOne("SharedLibrary.Models.ApplicationModel", "Application")
                        .WithMany("Rights")
                        .HasForeignKey("ApplicationId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("SharedLibrary.Models.UserModel", b =>
                {
                    b.HasOne("SharedLibrary.Models.ApplicationModel", "Application")
                        .WithMany("Users")
                        .HasForeignKey("ApplicationId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("SharedLibrary.Models.RightsModel", "Rights")
                        .WithMany("Users")
                        .HasForeignKey("RightsId")
                        .OnDelete(DeleteBehavior.Restrict);
                });
#pragma warning restore 612, 618
        }
    }
}
