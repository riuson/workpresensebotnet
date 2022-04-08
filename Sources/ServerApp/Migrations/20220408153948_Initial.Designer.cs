﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ServerApp.Database;

#nullable disable

namespace ServerApp.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20220408153948_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.3");

            modelBuilder.Entity("ServerApp.Entities.Chat", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<long>("ChatId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("chat_id");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("chats", (string)null);
                });

            modelBuilder.Entity("ServerApp.Entities.ChatStatus", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<long>("ChatId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("chat_id");

                    b.Property<Guid>("HookId")
                        .HasColumnType("TEXT")
                        .HasColumnName("hook_id");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER")
                        .HasColumnName("status");

                    b.Property<DateTime>("Time")
                        .HasColumnType("TEXT")
                        .HasColumnName("time");

                    b.HasKey("Id");

                    b.HasIndex("ChatId")
                        .IsUnique();

                    b.ToTable("statuses", (string)null);
                });

            modelBuilder.Entity("ServerApp.Entities.PhoneNumber", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("phone");

                    b.Property<long>("UserId")
                        .HasColumnType("INTEGER")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("phones", (string)null);
                });

            modelBuilder.Entity("ServerApp.Entities.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<string>("FirstName")
                        .HasColumnType("TEXT")
                        .HasColumnName("first_name");

                    b.Property<string>("LastName")
                        .HasColumnType("TEXT")
                        .HasColumnName("last_name");

                    b.Property<string>("NickName")
                        .HasColumnType("TEXT")
                        .HasColumnName("nickname");

                    b.HasKey("Id");

                    b.ToTable("users", (string)null);
                });

            modelBuilder.Entity("ServerApp.Entities.Chat", b =>
                {
                    b.HasOne("ServerApp.Entities.User", "User")
                        .WithMany("Chats")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("ServerApp.Entities.ChatStatus", b =>
                {
                    b.HasOne("ServerApp.Entities.Chat", "Chat")
                        .WithOne("Status")
                        .HasForeignKey("ServerApp.Entities.ChatStatus", "ChatId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Chat");
                });

            modelBuilder.Entity("ServerApp.Entities.PhoneNumber", b =>
                {
                    b.HasOne("ServerApp.Entities.User", "User")
                        .WithMany("Phones")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("ServerApp.Entities.Chat", b =>
                {
                    b.Navigation("Status")
                        .IsRequired();
                });

            modelBuilder.Entity("ServerApp.Entities.User", b =>
                {
                    b.Navigation("Chats");

                    b.Navigation("Phones");
                });
#pragma warning restore 612, 618
        }
    }
}
