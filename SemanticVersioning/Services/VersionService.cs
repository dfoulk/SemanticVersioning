﻿using SemanticVersioning.Extensions;
using SemanticVersioning.Models;
using System.Collections.Generic;
using System.Linq;

namespace SemanticVersioning.Services
{
    internal sealed class VersionService
    {
        private readonly EnvDTE.DTE _dte;

        private IEnumerable<Project> _projects;

        internal VersionService()
        {
            _dte = DteService.Instance.DTE;
        }

        private void LoadProjects()
        {
            var projects = new List<Project>();

            foreach (EnvDTE.Project project in _dte.Solution.Projects)
            {
                projects.Add(new Project(project));
            }

            _projects = projects;
        }

        internal Version GetHighestVersion()
        {
            LoadProjects();

            var versions = _projects.SelectMany(x => x.Files.Where(y => !y.Versions.IsNullOrEmpty()).SelectMany(y => y.Versions));

            var uniqueVersions = versions.GroupBy(x => new
            {
                x.Major,
                x.Minor,
                x.Patch,
                x.Build
            }).Select(x => x.FirstOrDefault());

            var sortedVersions = uniqueVersions
                .OrderByDescending(x => x.Major)
                .ThenByDescending(x => x.Minor)
                .ThenByDescending(x => x.Patch)
                .ThenByDescending(x => x.Build);

            var highestVersion = sortedVersions.FirstOrDefault();

            return highestVersion ?? new Version("1.0.0");
        }

        internal void SetVersions(Version version)
        {
            if (version.IsNullOrEmpty())
                throw new System.ArgumentException();

            LoadProjects();

            foreach (Project project in _projects)
            {
                foreach (IFile file in project.Files)
                {
                    file.SetVersions(version);
                }
            }
        }
    }
}
