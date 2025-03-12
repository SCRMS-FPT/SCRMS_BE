﻿// <auto-generated />
using System;
using System.Collections.Generic;
using CourtBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CourtBooking.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250312102215_FixBookingModelv2")]
    partial class FixBookingModelv2
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("CourtBooking.Domain.Models.Booking", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("BookingDate")
                        .HasColumnType("DATE");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("LastModified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Note")
                        .HasColumnType("TEXT");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<decimal>("TotalPrice")
                        .HasColumnType("DECIMAL");

                    b.Property<decimal>("TotalTime")
                        .HasColumnType("DECIMAL");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.ToTable("bookings", (string)null);
                });

            modelBuilder.Entity("CourtBooking.Domain.Models.BookingDetail", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<Guid>("BookingId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("CourtId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<TimeSpan>("EndTime")
                        .HasColumnType("TIME");

                    b.Property<DateTime?>("LastModified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<TimeSpan>("StartTime")
                        .HasColumnType("TIME");

                    b.Property<decimal>("TotalPrice")
                        .HasColumnType("DECIMAL");

                    b.HasKey("Id");

                    b.HasIndex("BookingId");

                    b.HasIndex("CourtId");

                    b.ToTable("booking_details", (string)null);
                });

            modelBuilder.Entity("CourtBooking.Domain.Models.Court", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<string>("CourtType")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text")
                        .HasDefaultValue("Indoor");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("Facilities")
                        .HasColumnType("JSONB");

                    b.Property<DateTime?>("LastModified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<double>("SlotDuration")
                        .HasColumnType("double precision");

                    b.Property<Guid>("SportCenterId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("SportId")
                        .HasColumnType("uuid");

                    b.Property<string>("Status")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text")
                        .HasDefaultValue("Open");

                    b.ComplexProperty<Dictionary<string, object>>("CourtName", "CourtBooking.Domain.Models.Court.CourtName#CourtName", b1 =>
                        {
                            b1.IsRequired();

                            b1.Property<string>("Value")
                                .IsRequired()
                                .HasMaxLength(255)
                                .HasColumnType("character varying(255)")
                                .HasColumnName("CourtName");
                        });

                    b.HasKey("Id");

                    b.HasIndex("SportCenterId");

                    b.HasIndex("SportId");

                    b.ToTable("courts", (string)null);
                });

            modelBuilder.Entity("CourtBooking.Domain.Models.CourtPromotion", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<Guid>("CourtId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("DiscountType")
                        .IsRequired()
                        .HasColumnType("VARCHAR(50)");

                    b.Property<decimal>("DiscountValue")
                        .HasColumnType("DECIMAL");

                    b.Property<DateTime?>("LastModified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("ValidFrom")
                        .HasColumnType("DATE");

                    b.Property<DateTime>("ValidTo")
                        .HasColumnType("DATE");

                    b.HasKey("Id");

                    b.HasIndex("CourtId");

                    b.ToTable("court_promotions", (string)null);
                });

            modelBuilder.Entity("CourtBooking.Domain.Models.CourtSchedule", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<Guid>("CourtId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int[]>("DayOfWeek")
                        .IsRequired()
                        .HasColumnType("integer[]");

                    b.Property<TimeSpan>("EndTime")
                        .HasColumnType("TIME");

                    b.Property<DateTime?>("LastModified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("PriceSlot")
                        .HasColumnType("DECIMAL");

                    b.Property<TimeSpan>("StartTime")
                        .HasColumnType("TIME");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CourtId");

                    b.ToTable("court_schedules", (string)null);
                });

            modelBuilder.Entity("CourtBooking.Domain.Models.Sport", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("Icon")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("LastModified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("Id");

                    b.ToTable("sports", (string)null);
                });

            modelBuilder.Entity("CourtBooking.Domain.Models.SportCenter", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("LastModified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<Guid>("OwnerId")
                        .HasColumnType("uuid");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasMaxLength(11)
                        .HasColumnType("character varying(11)");

                    b.ComplexProperty<Dictionary<string, object>>("Address", "CourtBooking.Domain.Models.SportCenter.Address#Location", b1 =>
                        {
                            b1.IsRequired();

                            b1.Property<string>("AddressLine")
                                .IsRequired()
                                .HasMaxLength(255)
                                .HasColumnType("character varying(255)");

                            b1.Property<string>("City")
                                .IsRequired()
                                .HasMaxLength(50)
                                .HasColumnType("character varying(50)");

                            b1.Property<string>("Commune")
                                .IsRequired()
                                .HasMaxLength(50)
                                .HasColumnType("character varying(50)");

                            b1.Property<string>("District")
                                .IsRequired()
                                .HasMaxLength(50)
                                .HasColumnType("character varying(50)");
                        });

                    b.ComplexProperty<Dictionary<string, object>>("LocationPoint", "CourtBooking.Domain.Models.SportCenter.LocationPoint#GeoLocation", b1 =>
                        {
                            b1.IsRequired();

                            b1.Property<double>("Latitude")
                                .HasColumnType("DOUBLE PRECISION");

                            b1.Property<double>("Longitude")
                                .HasColumnType("DOUBLE PRECISION");
                        });

                    b.HasKey("Id");

                    b.ToTable("sport_centers", (string)null);
                });

            modelBuilder.Entity("CourtBooking.Domain.Models.BookingDetail", b =>
                {
                    b.HasOne("CourtBooking.Domain.Models.Booking", null)
                        .WithMany("BookingDetails")
                        .HasForeignKey("BookingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CourtBooking.Domain.Models.Court", null)
                        .WithMany()
                        .HasForeignKey("CourtId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("CourtBooking.Domain.Models.Court", b =>
                {
                    b.HasOne("CourtBooking.Domain.Models.SportCenter", null)
                        .WithMany("Courts")
                        .HasForeignKey("SportCenterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CourtBooking.Domain.Models.Sport", null)
                        .WithMany()
                        .HasForeignKey("SportId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("CourtBooking.Domain.Models.CourtPromotion", b =>
                {
                    b.HasOne("CourtBooking.Domain.Models.Court", null)
                        .WithMany()
                        .HasForeignKey("CourtId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("CourtBooking.Domain.Models.CourtSchedule", b =>
                {
                    b.HasOne("CourtBooking.Domain.Models.Court", null)
                        .WithMany("CourtSchedules")
                        .HasForeignKey("CourtId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("CourtBooking.Domain.Models.SportCenter", b =>
                {
                    b.OwnsOne("CourtBooking.Domain.ValueObjects.SportCenterImages", "Images", b1 =>
                        {
                            b1.Property<Guid>("SportCenterId")
                                .HasColumnType("uuid");

                            b1.Property<string>("Avatar")
                                .IsRequired()
                                .HasMaxLength(500)
                                .HasColumnType("character varying(500)");

                            b1.Property<string>("ImageUrls")
                                .IsRequired()
                                .HasColumnType("jsonb");

                            b1.HasKey("SportCenterId");

                            b1.ToTable("sport_centers");

                            b1.WithOwner()
                                .HasForeignKey("SportCenterId");
                        });

                    b.Navigation("Images")
                        .IsRequired();
                });

            modelBuilder.Entity("CourtBooking.Domain.Models.Booking", b =>
                {
                    b.Navigation("BookingDetails");
                });

            modelBuilder.Entity("CourtBooking.Domain.Models.Court", b =>
                {
                    b.Navigation("CourtSchedules");
                });

            modelBuilder.Entity("CourtBooking.Domain.Models.SportCenter", b =>
                {
                    b.Navigation("Courts");
                });
#pragma warning restore 612, 618
        }
    }
}
