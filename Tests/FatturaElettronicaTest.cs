﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;
using FatturaElettronica.Impostazioni;
using System;

namespace Tests
{
    [TestClass]
    public class FatturaElettronicaTest
    {
        [TestMethod]
        public void CreateInstancePubblicaAmministrazione()
        {
            var f = FatturaElettronica.FatturaElettronica.CreateInstance(Instance.PubblicaAmministrazione);
            Assert.AreEqual(FormatoTrasmissione.PubblicaAmministrazione, f.FatturaElettronicaHeader.DatiTrasmissione.FormatoTrasmissione);
        }
        [TestMethod]
        public void CreateInstancePrivati()
        {
            var f = FatturaElettronica.FatturaElettronica.CreateInstance(Instance.Privati);
            Assert.AreEqual(FormatoTrasmissione.Privati, f.FatturaElettronicaHeader.DatiTrasmissione.FormatoTrasmissione);
            Assert.AreEqual(new string('0', 7), f.FatturaElettronicaHeader.DatiTrasmissione.CodiceDestinatario);
        }

        [TestMethod]
        public void SerializePrivatiHeader()
        {

            var result = SerializeAndGetBackVersionAndNamespace(
                FatturaElettronica.FatturaElettronica.CreateInstance(Instance.Privati));
            Assert.AreEqual(FormatoTrasmissione.Privati, result.Item1);
            Assert.AreEqual(RootElement.NameSpace, result.Item2);

        }

        [TestMethod]
        public void SerializePubblicaAmministrazioneHeader()
        {
            var result = SerializeAndGetBackVersionAndNamespace(
                FatturaElettronica.FatturaElettronica.CreateInstance(Instance.PubblicaAmministrazione));
            Assert.AreEqual(FormatoTrasmissione.PubblicaAmministrazione, result.Item1);
            Assert.AreEqual(RootElement.NameSpace, result.Item2);

        }
        [TestMethod]
        public void ValidateFormatoTrasmissione()
        {
            var f = FatturaElettronica.FatturaElettronica.CreateInstance(Instance.Privati);
            f.FatturaElettronicaHeader.DatiTrasmissione.FormatoTrasmissione = "test";
            Assert.IsTrue(f.Error.Contains("FormatoTrasmissione"));
            Assert.IsTrue(f.Error.Contains(FormatoTrasmissione.Privati));
            f.FatturaElettronicaHeader.DatiTrasmissione.FormatoTrasmissione =FormatoTrasmissione.Privati;
            Assert.IsFalse(f.Error.Contains("FormatoTrasmissione"));

            f = FatturaElettronica.FatturaElettronica.CreateInstance(Instance.PubblicaAmministrazione);
            f.FatturaElettronicaHeader.DatiTrasmissione.FormatoTrasmissione = "test";
            Assert.IsTrue(f.Error.Contains("FormatoTrasmissione"));
            Assert.IsTrue(f.Error.Contains(FormatoTrasmissione.PubblicaAmministrazione));
            f.FatturaElettronicaHeader.DatiTrasmissione.FormatoTrasmissione =FormatoTrasmissione.PubblicaAmministrazione;
            Assert.IsFalse(f.Error.Contains("FormatoTrasmissione"));
        }

        [TestMethod]
        public void ValidatePECDestinatario()
        {
            var f = FatturaElettronica.FatturaElettronica.CreateInstance(Instance.Privati);

            // Nelle fatture tra privati, poiché il default per CodiceDestinatario è 0000000,
            // è necessario impostare un valore per PECDestinatario.
            Assert.IsTrue(f.Error.Contains("PECDestinatario"));

            // PECDestinatario deve essere tra 7 e 256 caratteri.
            f.FatturaElettronicaHeader.DatiTrasmissione.PECDestinatario = "123456";
            Assert.IsTrue(f.Error.Contains("PECDestinatario"));
            f.FatturaElettronicaHeader.DatiTrasmissione.PECDestinatario = new string('n', 257);
            Assert.IsTrue(f.Error.Contains("PECDestinatario"));
            f.FatturaElettronicaHeader.DatiTrasmissione.PECDestinatario = "1234567";
            Assert.IsFalse(f.Error.Contains("PECDestinatario"));

            // Se CodiceDestinatario è diverso da 0000000, allora PECDestinatario non deve essere
            // valorizzato
            f.FatturaElettronicaHeader.DatiTrasmissione.CodiceDestinatario = "test";
            Assert.IsTrue(f.Error.Contains("PECDestinatario"));
            f.FatturaElettronicaHeader.DatiTrasmissione.PECDestinatario = null;
            Assert.IsFalse(f.Error.Contains("PECDestinatario"));


        }

        private Tuple<string, string> SerializeAndGetBackVersionAndNamespace(FatturaElettronica.FatturaElettronica f)
        {
            using (var w = XmlWriter.Create("test", new XmlWriterSettings { Indent = true }))
            {
                f.WriteXml(w);
            }

            var version = string.Empty;
            var xmlns = string.Empty;
            using (var r = XmlReader.Create("test"))
            {
               while (r.Read())
                {
                    if (r.NodeType == XmlNodeType.Element)
                    {
                        if (r.Prefix == RootElement.Prefix && r.LocalName == RootElement.LocalName)
                        {
                            version = r.GetAttribute("versione");
                            xmlns = r.NamespaceURI;
                            break;
                        }
                    }
                }
            }
            return new Tuple<string, string>(version, xmlns);
        }
    }
}
