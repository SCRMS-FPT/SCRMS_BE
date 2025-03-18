﻿// <auto-generated />
using System;
using Matching.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Matching.API.Migrations
{
    [DbContext(typeof(MatchingDbContext))]
    partial class MatchingDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("BuildingBlocks.Messaging.Outbox.OutboxMessage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Error")
                        .HasColumnType("text");

                    b.Property<DateTime?>("ProcessedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ProcessedAt");

                    b.ToTable("OutboxMessages");
                });

            modelBuilder.Entity("Matching.API.Data.Models.Match", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("InitiatorId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("MatchTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("MatchedUserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.ToTable("Matches");
                });

            modelBuilder.Entity("Matching.API.Data.Models.SwipeAction", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Decision")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)");

                    b.Property<Guid>("SwipedUserId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("SwiperId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("SwiperId", "SwipedUserId");

                    b.ToTable("SwipeActions");
                });

            modelBuilder.Entity("Matching.API.Data.Models.UserSkill", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("SportId")
                        .HasColumnType("uuid");

                    b.Property<string>("SkillLevel")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.HasKey("UserId", "SportId");

                    b.ToTable("UserSkills");
                });
#pragma warning restore 612, 618
        }
    }
}
