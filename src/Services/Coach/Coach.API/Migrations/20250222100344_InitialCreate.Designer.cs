﻿// <auto-generated />
using System;
using Coach.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Coach.API.Migrations
{
    [DbContext(typeof(CoachDbContext))]
    [Migration("20250222100344_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Coach.API.Models.Coach", b =>
                {
                    b.Property<Guid>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Bio")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("RatePerHour")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("UserId");

                    b.ToTable("Coaches");
                });

            modelBuilder.Entity("Coach.API.Models.CoachBooking", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateOnly>("BookingDate")
                        .HasColumnType("date");

                    b.Property<Guid>("CoachId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<TimeOnly>("EndTime")
                        .HasColumnType("time without time zone");

                    b.Property<Guid?>("PackageId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("SportId")
                        .HasColumnType("uuid");

                    b.Property<TimeOnly>("StartTime")
                        .HasColumnType("time without time zone");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<decimal>("TotalPrice")
                        .HasColumnType("decimal(18,2)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("CoachId");

                    b.HasIndex("PackageId");

                    b.ToTable("CoachBookings");
                });

            modelBuilder.Entity("Coach.API.Models.CoachPackage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("CoachId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("CoachUserId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("SessionCount")
                        .HasColumnType("integer");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("CoachId");

                    b.HasIndex("CoachUserId");

                    b.ToTable("CoachPackages");
                });

            modelBuilder.Entity("Coach.API.Models.CoachSchedule", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("CoachId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("CoachUserId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("DayOfWeek")
                        .HasColumnType("integer");

                    b.Property<TimeOnly>("EndTime")
                        .HasColumnType("time without time zone");

                    b.Property<TimeOnly>("StartTime")
                        .HasColumnType("time without time zone");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("CoachId");

                    b.HasIndex("CoachUserId");

                    b.ToTable("CoachSchedules");
                });

            modelBuilder.Entity("Coach.API.Models.CoachSport", b =>
                {
                    b.Property<Guid>("CoachId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("SportId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("CoachId", "SportId");

                    b.ToTable("CoachSports");
                });

            modelBuilder.Entity("Coach.API.Models.CoachBooking", b =>
                {
                    b.HasOne("Coach.API.Models.Coach", null)
                        .WithMany()
                        .HasForeignKey("CoachId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Coach.API.Models.CoachPackage", null)
                        .WithMany()
                        .HasForeignKey("PackageId");
                });

            modelBuilder.Entity("Coach.API.Models.CoachPackage", b =>
                {
                    b.HasOne("Coach.API.Models.Coach", null)
                        .WithMany()
                        .HasForeignKey("CoachId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Coach.API.Models.Coach", null)
                        .WithMany("Packages")
                        .HasForeignKey("CoachUserId");
                });

            modelBuilder.Entity("Coach.API.Models.CoachSchedule", b =>
                {
                    b.HasOne("Coach.API.Models.Coach", null)
                        .WithMany()
                        .HasForeignKey("CoachId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Coach.API.Models.Coach", null)
                        .WithMany("Schedules")
                        .HasForeignKey("CoachUserId");
                });

            modelBuilder.Entity("Coach.API.Models.CoachSport", b =>
                {
                    b.HasOne("Coach.API.Models.Coach", "Coach")
                        .WithMany("Sports")
                        .HasForeignKey("CoachId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Coach");
                });

            modelBuilder.Entity("Coach.API.Models.Coach", b =>
                {
                    b.Navigation("Packages");

                    b.Navigation("Schedules");

                    b.Navigation("Sports");
                });
#pragma warning restore 612, 618
        }
    }
}
