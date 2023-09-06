﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SussyKart_Partie1.Models;

namespace SussyKart_Partie1.Data
{
    public partial class TP2_SussyKartContext : DbContext
    {
        public TP2_SussyKartContext()
        {
        }

        public TP2_SussyKartContext(DbContextOptions<TP2_SussyKartContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Amitie> Amities { get; set; } = null!;
        public virtual DbSet<Avatar> Avatars { get; set; } = null!;
        public virtual DbSet<Changelog> Changelogs { get; set; } = null!;
        public virtual DbSet<Course> Courses { get; set; } = null!;
        public virtual DbSet<NoBancaire> NoBancaires { get; set; } = null!;
        public virtual DbSet<ParticipationCourse> ParticipationCourses { get; set; } = null!;
        public virtual DbSet<Utilisateur> Utilisateurs { get; set; } = null!;
        public virtual DbSet<VwToutesParticipation> VwToutesParticipations { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Name=SussyKart");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Amitie>(entity =>
            {
                entity.HasOne(d => d.Utilisateur)
                    .WithMany(p => p.AmitieUtilisateurs)
                    .HasForeignKey(d => d.UtilisateurId)
                    .HasConstraintName("FK_Amitie_UtilisateurID");

                entity.HasOne(d => d.UtilisateurIdAmiNavigation)
                    .WithMany(p => p.AmitieUtilisateurIdAmiNavigations)
                    .HasForeignKey(d => d.UtilisateurIdAmi)
                    .HasConstraintName("FK_Amitie_UtilisateurID_Ami");
            });

            modelBuilder.Entity<Avatar>(entity =>
            {
                entity.Property(e => e.Identifiant).HasDefaultValueSql("(newid())");

                entity.HasOne(d => d.Utilisateur)
                    .WithOne(p => p.Avatar)
                    .HasForeignKey<Avatar>(d => d.UtilisateurId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Avatar_UtilisateurID");
            });

            modelBuilder.Entity<Changelog>(entity =>
            {
                entity.Property(e => e.InstalledOn).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<ParticipationCourse>(entity =>
            {
                entity.HasOne(d => d.Course)
                    .WithMany(p => p.ParticipationCourses)
                    .HasForeignKey(d => d.CourseId)
                    .HasConstraintName("FK_ParticipationCourse_CourseID");

                entity.HasOne(d => d.Utilisateur)
                    .WithMany(p => p.ParticipationCourses)
                    .HasForeignKey(d => d.UtilisateurId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ParticipationCourse_UtilisateurID");
            });

            modelBuilder.Entity<Utilisateur>(entity =>
            {
                entity.Property(e => e.MotDePasse).HasDefaultValueSql("(0x)");

                entity.Property(e => e.NoBancaire).HasDefaultValueSql("(0x)");

                entity.Property(e => e.Sel).HasDefaultValueSql("(0x)");
            });

            modelBuilder.Entity<VwToutesParticipation>(entity =>
            {
                entity.ToView("VW_ToutesParticipations", "Courses");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
