﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EnvDTE;
using Logic.Integration;
using Logic.SemanticTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    static class TestExtesions
    {
        public static void RemoveChild(this Node tree, string name)
        {
            var withName = tree.Childs.WithName(name);
            if (withName == null)
                throw new ChildNotFoundException(name);
            foreach (var child in tree.Childs.Where(child => child.Dependencies.Contains(withName)))
            {
                child.Dependencies.Remove(withName);
            }
            tree.RemoveChild(withName);
        }
        
        public static string SubStrBetween(this string str, int startIndex,int endIndex)
        {
            return str.Substring(startIndex, endIndex - startIndex);
        }

        public static DTE Dte => (DTE)Marshal.GetActiveObject("VisualStudio.DTE.14.0");
        public static string SlnDir => new AdvancedSolution(Dte).Directory();
        public static readonly AdvancedSolution TestSolution = new AdvancedSolution(Dte);


        public static Node BuildTree(this string text)
        {
            text = Regex.Replace(text, @"\s", "");
            var root = new Node("Tree");
            root.SetChildren(_BuildTree(text));
            return root;
        }

        private static IEnumerable<Node> _BuildTree(this string text)
        {
            var nestedChildStart = 0;
            var depth = 0;
            var currentChildStartIndex = 0;
            var childEntries = new List<string>();
            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (c == '[')
                {
                    if (depth == 0)
                        nestedChildStart = i;
                    depth++;
                }
                if (c == ']')
                {
                    depth--;
                    if (depth == 0)
                    {
                        childEntries.Add(text.SubStrBetween(nestedChildStart, i + 1));
                        currentChildStartIndex = i + 1;
                    }
                }
                else if (c == ',' && depth == 0)
                {
                    childEntries.Add(text.SubStrBetween(currentChildStartIndex, i));
                    currentChildStartIndex = i + 1;
                }
            }
            if(currentChildStartIndex != text.Length)
                childEntries.Add(text.SubStrBetween(currentChildStartIndex, text.Length));

            foreach (var entry in childEntries)
            {
                if (entry.StartsWith("[") && entry.EndsWith("]"))
                {
                    var node = new Node("");
                    node.SetChildren(_BuildTree(entry.SubStrBetween(1, entry.Length - 1)));
                    yield return node;
                }
                else
                {
                    yield return new Node(entry);
                }
            }

        }
    }

    internal class ChildNotFoundException : Exception
    {
        public ChildNotFoundException(string message) : base(message)
        {
        }
    }
}
