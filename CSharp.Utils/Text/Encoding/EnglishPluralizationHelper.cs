using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CSharp.Utils.Collections.Generic;
using CSharp.Utils.Validation;

namespace CSharp.Utils.Text.Encoding
{
    public static class EnglishPluralizationHelper
    {
        private static CultureInfo _culture = new CultureInfo("en");

        private static BidirectionalDictionary<string, string> _userDictionary;

        private static StringBidirectionalDictionary _irregularPluralsPluralizationService;

        private static StringBidirectionalDictionary _assimilatedClassicalInflectionPluralizationService;

        private static StringBidirectionalDictionary _oSuffixPluralizationService;

        private static StringBidirectionalDictionary _classicalInflectionPluralizationService;

        private static StringBidirectionalDictionary _irregularVerbPluralizationService;

        private static StringBidirectionalDictionary _wordsEndingWithSePluralizationService;

        private static StringBidirectionalDictionary _wordsEndingWithSisPluralizationService;

        private static StringBidirectionalDictionary _wordsEndingWithSusPluralizationService;

        private static StringBidirectionalDictionary _wordsEndingWithInxAnxYnxPluralizationService;

        private static List<string> _knownSingluarWords;

        private static List<string> _knownPluralWords;

        private static string[] _uninflectiveSuffixList = new[]
        {
            "fish", 
            "ois", 
            "sheep", 
            "deer", 
            "pos", 
            "itis", 
            "ism"
        };

        private static readonly string[] _uninflectiveWordList = new[]
        {
            "bison", 
            "flounder", 
            "pliers", 
            "bream", 
            "gallows", 
            "proceedings", 
            "breeches", 
            "graffiti", 
            "rabies", 
            "britches", 
            "headquarters", 
            "salmon", 
            "carp", 
            "herpes", 
            "scissors", 
            "chassis", 
            "high-jinks", 
            "sea-bass", 
            "clippers", 
            "homework", 
            "series", 
            "cod", 
            "innings", 
            "shears", 
            "contretemps", 
            "jackanapes", 
            "species", 
            "corps", 
            "mackerel", 
            "swine", 
            "debris", 
            "measles", 
            "trout", 
            "diabetes", 
            "mews", 
            "tuna", 
            "djinn", 
            "mumps", 
            "whiting", 
            "eland", 
            "news", 
            "wildebeest", 
            "elk", 
            "pincers", 
            "police", 
            "hair", 
            "ice", 
            "chaos", 
            "milk", 
            "cotton", 
            "pneumonoultramicroscopicsilicovolcanoconiosis", 
            "information", 
            "aircraft", 
            "scabies", 
            "traffic", 
            "corn", 
            "millet", 
            "rice", 
            "hay", 
            "hemp", 
            "tobacco", 
            "cabbage", 
            "okra", 
            "broccoli", 
            "asparagus", 
            "lettuce", 
            "beef", 
            "pork", 
            "venison", 
            "mutton", 
            "cattle", 
            "offspring", 
            "molasses", 
            "shambles", 
            "shingles"
        };

        private static Dictionary<string, string> _irregularVerbList = new Dictionary<string, string>
        {
            {
                "am", 
                "are"
            }, 
            {
                "are", 
                "are"
            }, 
            {
                "is", 
                "are"
            }, 
            {
                "was", 
                "were"
            }, 
            {
                "were", 
                "were"
            }, 
            {
                "has", 
                "have"
            }, 
            {
                "have", 
                "have"
            }
        };

        private static List<string> _pronounList = new List<string>
        {
            "I", 
            "we", 
            "you", 
            "he", 
            "she", 
            "they", 
            "it", 
            "me", 
            "us", 
            "him", 
            "her", 
            "them", 
            "myself", 
            "ourselves", 
            "yourself", 
            "himself", 
            "herself", 
            "itself", 
            "oneself", 
            "oneselves", 
            "my", 
            "our", 
            "your", 
            "his", 
            "their", 
            "its", 
            "mine", 
            "yours", 
            "hers", 
            "theirs", 
            "this", 
            "that", 
            "these", 
            "those", 
            "all", 
            "another", 
            "any", 
            "anybody", 
            "anyone", 
            "anything", 
            "both", 
            "each", 
            "other", 
            "either", 
            "everyone", 
            "everybody", 
            "everything", 
            "most", 
            "much", 
            "nothing", 
            "nobody", 
            "none", 
            "one", 
            "others", 
            "some", 
            "somebody", 
            "someone", 
            "something", 
            "what", 
            "whatever", 
            "which", 
            "whichever", 
            "who", 
            "whoever", 
            "whom", 
            "whomever", 
            "whose"
        };

        private static Dictionary<string, string> _irregularPluralsDictionary = new Dictionary<string, string>
        {
            {
                "brother", 
                "brothers"
            }, 
            {
                "child", 
                "children"
            }, 
            {
                "cow", 
                "cows"
            }, 
            {
                "ephemeris", 
                "ephemerides"
            }, 
            {
                "genie", 
                "genies"
            }, 
            {
                "money", 
                "moneys"
            }, 
            {
                "mongoose", 
                "mongooses"
            }, 
            {
                "mythos", 
                "mythoi"
            }, 
            {
                "octopus", 
                "octopuses"
            }, 
            {
                "ox", 
                "oxen"
            }, 
            {
                "soliloquy", 
                "soliloquies"
            }, 
            {
                "trilby", 
                "trilbys"
            }, 
            {
                "crisis", 
                "crises"
            }, 
            {
                "synopsis", 
                "synopses"
            }, 
            {
                "rose", 
                "roses"
            }, 
            {
                "gas", 
                "gases"
            }, 
            {
                "bus", 
                "buses"
            }, 
            {
                "axis", 
                "axes"
            }, 
            {
                "memo", 
                "memos"
            }, 
            {
                "casino", 
                "casinos"
            }, 
            {
                "silo", 
                "silos"
            }, 
            {
                "stereo", 
                "stereos"
            }, 
            {
                "studio", 
                "studios"
            }, 
            {
                "lens", 
                "lenses"
            }, 
            {
                "alias", 
                "aliases"
            }, 
            {
                "pie", 
                "pies"
            }, 
            {
                "corpus", 
                "corpora"
            }, 
            {
                "viscus", 
                "viscera"
            }, 
            {
                "hippopotamus", 
                "hippopotami"
            }, 
            {
                "trace", 
                "traces"
            }, 
            {
                "person", 
                "people"
            }, 
            {
                "chili", 
                "chilies"
            }, 
            {
                "analysis", 
                "analyses"
            }, 
            {
                "basis", 
                "bases"
            }, 
            {
                "neurosis", 
                "neuroses"
            }, 
            {
                "oasis", 
                "oases"
            }, 
            {
                "synthesis", 
                "syntheses"
            }, 
            {
                "thesis", 
                "theses"
            }, 
            {
                "change", 
                "changes"
            }, 
            {
                "lie", 
                "lies"
            }, 
            {
                "calorie", 
                "calories"
            }, 
            {
                "freebie", 
                "freebies"
            }, 
            {
                "case", 
                "cases"
            }, 
            {
                "house", 
                "houses"
            }, 
            {
                "valve", 
                "valves"
            }, 
            {
                "cloth", 
                "clothes"
            }, 
            {
                "tie", 
                "ties"
            }, 
            {
                "movie", 
                "movies"
            }, 
            {
                "bonus", 
                "bonuses"
            }, 
            {
                "specimen", 
                "specimens"
            }
        };

        private static Dictionary<string, string> _assimilatedClassicalInflectionDictionary = new Dictionary
            <string, string>
        {
            {
                "alumna", 
                "alumnae"
            }, 
            {
                "alga", 
                "algae"
            }, 
            {
                "vertebra", 
                "vertebrae"
            }, 
            {
                "codex", 
                "codices"
            }, 
            {
                "murex", 
                "murices"
            }, 
            {
                "silex", 
                "silices"
            }, 
            {
                "aphelion", 
                "aphelia"
            }, 
            {
                "hyperbaton", 
                "hyperbata"
            }, 
            {
                "perihelion", 
                "perihelia"
            }, 
            {
                "asyndeton", 
                "asyndeta"
            }, 
            {
                "noumenon", 
                "noumena"
            }, 
            {
                "phenomenon", 
                "phenomena"
            }, 
            {
                "criterion", 
                "criteria"
            }, 
            {
                "organon", 
                "organa"
            }, 
            {
                "prolegomenon", 
                "prolegomena"
            }, 
            {
                "agendum", 
                "agenda"
            }, 
            {
                "datum", 
                "data"
            }, 
            {
                "extremum", 
                "extrema"
            }, 
            {
                "bacterium", 
                "bacteria"
            }, 
            {
                "desideratum", 
                "desiderata"
            }, 
            {
                "stratum", 
                "strata"
            }, 
            {
                "candelabrum", 
                "candelabra"
            }, 
            {
                "erratum", 
                "errata"
            }, 
            {
                "ovum", 
                "ova"
            }, 
            {
                "forum", 
                "fora"
            }, 
            {
                "addendum", 
                "addenda"
            }, 
            {
                "stadium", 
                "stadia"
            }, 
            {
                "automaton", 
                "automata"
            }, 
            {
                "polyhedron", 
                "polyhedra"
            }
        };

        private static Dictionary<string, string> _oSuffixDictionary = new Dictionary<string, string>
        {
            {
                "albino", 
                "albinos"
            }, 
            {
                "generalissimo", 
                "generalissimos"
            }, 
            {
                "manifesto", 
                "manifestos"
            }, 
            {
                "archipelago", 
                "archipelagos"
            }, 
            {
                "ghetto", 
                "ghettos"
            }, 
            {
                "medico", 
                "medicos"
            }, 
            {
                "armadillo", 
                "armadillos"
            }, 
            {
                "guano", 
                "guanos"
            }, 
            {
                "octavo", 
                "octavos"
            }, 
            {
                "commando", 
                "commandos"
            }, 
            {
                "inferno", 
                "infernos"
            }, 
            {
                "photo", 
                "photos"
            }, 
            {
                "ditto", 
                "dittos"
            }, 
            {
                "jumbo", 
                "jumbos"
            }, 
            {
                "pro", 
                "pros"
            }, 
            {
                "dynamo", 
                "dynamos"
            }, 
            {
                "lingo", 
                "lingos"
            }, 
            {
                "quarto", 
                "quartos"
            }, 
            {
                "embryo", 
                "embryos"
            }, 
            {
                "lumbago", 
                "lumbagos"
            }, 
            {
                "rhino", 
                "rhinos"
            }, 
            {
                "fiasco", 
                "fiascos"
            }, 
            {
                "magneto", 
                "magnetos"
            }, 
            {
                "stylo", 
                "stylos"
            }
        };

        private static Dictionary<string, string> _classicalInflectionDictionary = new Dictionary<string, string>
        {
            {
                "stamen", 
                "stamina"
            }, 
            {
                "foramen", 
                "foramina"
            }, 
            {
                "lumen", 
                "lumina"
            }, 
            {
                "anathema", 
                "anathemata"
            }, 
            {
                "enema", 
                "enemata"
            }, 
            {
                "oedema", 
                "oedemata"
            }, 
            {
                "bema", 
                "bemata"
            }, 
            {
                "enigma", 
                "enigmata"
            }, 
            {
                "sarcoma", 
                "sarcomata"
            }, 
            {
                "carcinoma", 
                "carcinomata"
            }, 
            {
                "gumma", 
                "gummata"
            }, 
            {
                "schema", 
                "schemata"
            }, 
            {
                "charisma", 
                "charismata"
            }, 
            {
                "lemma", 
                "lemmata"
            }, 
            {
                "soma", 
                "somata"
            }, 
            {
                "diploma", 
                "diplomata"
            }, 
            {
                "lymphoma", 
                "lymphomata"
            }, 
            {
                "stigma", 
                "stigmata"
            }, 
            {
                "dogma", 
                "dogmata"
            }, 
            {
                "magma", 
                "magmata"
            }, 
            {
                "stoma", 
                "stomata"
            }, 
            {
                "drama", 
                "dramata"
            }, 
            {
                "melisma", 
                "melismata"
            }, 
            {
                "trauma", 
                "traumata"
            }, 
            {
                "edema", 
                "edemata"
            }, 
            {
                "miasma", 
                "miasmata"
            }, 
            {
                "abscissa", 
                "abscissae"
            }, 
            {
                "formula", 
                "formulae"
            }, 
            {
                "medusa", 
                "medusae"
            }, 
            {
                "amoeba", 
                "amoebae"
            }, 
            {
                "hydra", 
                "hydrae"
            }, 
            {
                "nebula", 
                "nebulae"
            }, 
            {
                "antenna", 
                "antennae"
            }, 
            {
                "hyperbola", 
                "hyperbolae"
            }, 
            {
                "nova", 
                "novae"
            }, 
            {
                "aurora", 
                "aurorae"
            }, 
            {
                "lacuna", 
                "lacunae"
            }, 
            {
                "parabola", 
                "parabolae"
            }, 
            {
                "apex", 
                "apices"
            }, 
            {
                "latex", 
                "latices"
            }, 
            {
                "vertex", 
                "vertices"
            }, 
            {
                "cortex", 
                "cortices"
            }, 
            {
                "pontifex", 
                "pontifices"
            }, 
            {
                "vortex", 
                "vortices"
            }, 
            {
                "index", 
                "indices"
            }, 
            {
                "simplex", 
                "simplices"
            }, 
            {
                "iris", 
                "irides"
            }, 
            {
                "clitoris", 
                "clitorides"
            }, 
            {
                "alto", 
                "alti"
            }, 
            {
                "contralto", 
                "contralti"
            }, 
            {
                "soprano", 
                "soprani"
            }, 
            {
                "basso", 
                "bassi"
            }, 
            {
                "crescendo", 
                "crescendi"
            }, 
            {
                "tempo", 
                "tempi"
            }, 
            {
                "canto", 
                "canti"
            }, 
            {
                "solo", 
                "soli"
            }, 
            {
                "aquarium", 
                "aquaria"
            }, 
            {
                "interregnum", 
                "interregna"
            }, 
            {
                "quantum", 
                "quanta"
            }, 
            {
                "compendium", 
                "compendia"
            }, 
            {
                "lustrum", 
                "lustra"
            }, 
            {
                "rostrum", 
                "rostra"
            }, 
            {
                "consortium", 
                "consortia"
            }, 
            {
                "maximum", 
                "maxima"
            }, 
            {
                "spectrum", 
                "spectra"
            }, 
            {
                "cranium", 
                "crania"
            }, 
            {
                "medium", 
                "media"
            }, 
            {
                "speculum", 
                "specula"
            }, 
            {
                "curriculum", 
                "curricula"
            }, 
            {
                "memorandum", 
                "memoranda"
            }, 
            {
                "stadium", 
                "stadia"
            }, 
            {
                "dictum", 
                "dicta"
            }, 
            {
                "millenium", 
                "millenia"
            }, 
            {
                "trapezium", 
                "trapezia"
            }, 
            {
                "emporium", 
                "emporia"
            }, 
            {
                "minimum", 
                "minima"
            }, 
            {
                "ultimatum", 
                "ultimata"
            }, 
            {
                "enconium", 
                "enconia"
            }, 
            {
                "momentum", 
                "momenta"
            }, 
            {
                "vacuum", 
                "vacua"
            }, 
            {
                "gymnasium", 
                "gymnasia"
            }, 
            {
                "optimum", 
                "optima"
            }, 
            {
                "velum", 
                "vela"
            }, 
            {
                "honorarium", 
                "honoraria"
            }, 
            {
                "phylum", 
                "phyla"
            }, 
            {
                "focus", 
                "foci"
            }, 
            {
                "nimbus", 
                "nimbi"
            }, 
            {
                "succubus", 
                "succubi"
            }, 
            {
                "fungus", 
                "fungi"
            }, 
            {
                "nucleolus", 
                "nucleoli"
            }, 
            {
                "torus", 
                "tori"
            }, 
            {
                "genius", 
                "genii"
            }, 
            {
                "radius", 
                "radii"
            }, 
            {
                "umbilicus", 
                "umbilici"
            }, 
            {
                "incubus", 
                "incubi"
            }, 
            {
                "stylus", 
                "styli"
            }, 
            {
                "uterus", 
                "uteri"
            }, 
            {
                "stimulus", 
                "stimuli"
            }, 
            {
                "apparatus", 
                "apparatus"
            }, 
            {
                "impetus", 
                "impetus"
            }, 
            {
                "prospectus", 
                "prospectus"
            }, 
            {
                "cantus", 
                "cantus"
            }, 
            {
                "nexus", 
                "nexus"
            }, 
            {
                "sinus", 
                "sinus"
            }, 
            {
                "coitus", 
                "coitus"
            }, 
            {
                "plexus", 
                "plexus"
            }, 
            {
                "status", 
                "status"
            }, 
            {
                "hiatus", 
                "hiatus"
            }, 
            {
                "afreet", 
                "afreeti"
            }, 
            {
                "afrit", 
                "afriti"
            }, 
            {
                "efreet", 
                "efreeti"
            }, 
            {
                "cherub", 
                "cherubim"
            }, 
            {
                "goy", 
                "goyim"
            }, 
            {
                "seraph", 
                "seraphim"
            }, 
            {
                "alumnus", 
                "alumni"
            }
        };

        private static List<string> _knownConflictingPluralList = new List<string>
        {
            "they", 
            "them", 
            "their", 
            "have", 
            "were", 
            "yourself", 
            "are"
        };

        private static Dictionary<string, string> _wordsEndingWithSeDictionary = new Dictionary<string, string>
        {
            {
                "house", 
                "houses"
            }, 
            {
                "case", 
                "cases"
            }, 
            {
                "enterprise", 
                "enterprises"
            }, 
            {
                "purchase", 
                "purchases"
            }, 
            {
                "surprise", 
                "surprises"
            }, 
            {
                "release", 
                "releases"
            }, 
            {
                "disease", 
                "diseases"
            }, 
            {
                "promise", 
                "promises"
            }, 
            {
                "refuse", 
                "refuses"
            }, 
            {
                "whose", 
                "whoses"
            }, 
            {
                "phase", 
                "phases"
            }, 
            {
                "noise", 
                "noises"
            }, 
            {
                "nurse", 
                "nurses"
            }, 
            {
                "rose", 
                "roses"
            }, 
            {
                "franchise", 
                "franchises"
            }, 
            {
                "supervise", 
                "supervises"
            }, 
            {
                "farmhouse", 
                "farmhouses"
            }, 
            {
                "suitcase", 
                "suitcases"
            }, 
            {
                "recourse", 
                "recourses"
            }, 
            {
                "impulse", 
                "impulses"
            }, 
            {
                "license", 
                "licenses"
            }, 
            {
                "diocese", 
                "dioceses"
            }, 
            {
                "excise", 
                "excises"
            }, 
            {
                "demise", 
                "demises"
            }, 
            {
                "blouse", 
                "blouses"
            }, 
            {
                "bruise", 
                "bruises"
            }, 
            {
                "misuse", 
                "misuses"
            }, 
            {
                "curse", 
                "curses"
            }, 
            {
                "prose", 
                "proses"
            }, 
            {
                "purse", 
                "purses"
            }, 
            {
                "goose", 
                "gooses"
            }, 
            {
                "tease", 
                "teases"
            }, 
            {
                "poise", 
                "poises"
            }, 
            {
                "vase", 
                "vases"
            }, 
            {
                "fuse", 
                "fuses"
            }, 
            {
                "muse", 
                "muses"
            }, 
            {
                "slaughterhouse", 
                "slaughterhouses"
            }, 
            {
                "clearinghouse", 
                "clearinghouses"
            }, 
            {
                "endonuclease", 
                "endonucleases"
            }, 
            {
                "steeplechase", 
                "steeplechases"
            }, 
            {
                "metamorphose", 
                "metamorphoses"
            }, 
            {
                "intercourse", 
                "intercourses"
            }, 
            {
                "commonsense", 
                "commonsenses"
            }, 
            {
                "intersperse", 
                "intersperses"
            }, 
            {
                "merchandise", 
                "merchandises"
            }, 
            {
                "phosphatase", 
                "phosphatases"
            }, 
            {
                "summerhouse", 
                "summerhouses"
            }, 
            {
                "watercourse", 
                "watercourses"
            }, 
            {
                "catchphrase", 
                "catchphrases"
            }, 
            {
                "compromise", 
                "compromises"
            }, 
            {
                "greenhouse", 
                "greenhouses"
            }, 
            {
                "lighthouse", 
                "lighthouses"
            }, 
            {
                "paraphrase", 
                "paraphrases"
            }, 
            {
                "mayonnaise", 
                "mayonnaises"
            }, 
            {
                "racecourse", 
                "racecourses"
            }, 
            {
                "apocalypse", 
                "apocalypses"
            }, 
            {
                "courthouse", 
                "courthouses"
            }, 
            {
                "powerhouse", 
                "powerhouses"
            }, 
            {
                "storehouse", 
                "storehouses"
            }, 
            {
                "glasshouse", 
                "glasshouses"
            }, 
            {
                "hypotenuse", 
                "hypotenuses"
            }, 
            {
                "peroxidase", 
                "peroxidases"
            }, 
            {
                "pillowcase", 
                "pillowcases"
            }, 
            {
                "roundhouse", 
                "roundhouses"
            }, 
            {
                "streetwise", 
                "streetwises"
            }, 
            {
                "expertise", 
                "expertises"
            }, 
            {
                "discourse", 
                "discourses"
            }, 
            {
                "warehouse", 
                "warehouses"
            }, 
            {
                "staircase", 
                "staircases"
            }, 
            {
                "workhouse", 
                "workhouses"
            }, 
            {
                "briefcase", 
                "briefcases"
            }, 
            {
                "clubhouse", 
                "clubhouses"
            }, 
            {
                "clockwise", 
                "clockwises"
            }, 
            {
                "concourse", 
                "concourses"
            }, 
            {
                "playhouse", 
                "playhouses"
            }, 
            {
                "turquoise", 
                "turquoises"
            }, 
            {
                "boathouse", 
                "boathouses"
            }, 
            {
                "cellulose", 
                "celluloses"
            }, 
            {
                "epitomise", 
                "epitomises"
            }, 
            {
                "gatehouse", 
                "gatehouses"
            }, 
            {
                "grandiose", 
                "grandioses"
            }, 
            {
                "menopause", 
                "menopauses"
            }, 
            {
                "penthouse", 
                "penthouses"
            }, 
            {
                "racehorse", 
                "racehorses"
            }, 
            {
                "transpose", 
                "transposes"
            }, 
            {
                "almshouse", 
                "almshouses"
            }, 
            {
                "customise", 
                "customises"
            }, 
            {
                "footloose", 
                "footlooses"
            }, 
            {
                "galvanise", 
                "galvanises"
            }, 
            {
                "princesse", 
                "princesses"
            }, 
            {
                "universe", 
                "universes"
            }, 
            {
                "workhorse", 
                "workhorses"
            }
        };

        private static Dictionary<string, string> _wordsEndingWithSisDictionary = new Dictionary<string, string>
        {
            {
                "analysis", 
                "analyses"
            }, 
            {
                "crisis", 
                "crises"
            }, 
            {
                "basis", 
                "bases"
            }, 
            {
                "atherosclerosis", 
                "atheroscleroses"
            }, 
            {
                "electrophoresis", 
                "electrophoreses"
            }, 
            {
                "psychoanalysis", 
                "psychoanalyses"
            }, 
            {
                "photosynthesis", 
                "photosyntheses"
            }, 
            {
                "amniocentesis", 
                "amniocenteses"
            }, 
            {
                "metamorphosis", 
                "metamorphoses"
            }, 
            {
                "toxoplasmosis", 
                "toxoplasmoses"
            }, 
            {
                "endometriosis", 
                "endometrioses"
            }, 
            {
                "tuberculosis", 
                "tuberculoses"
            }, 
            {
                "pathogenesis", 
                "pathogeneses"
            }, 
            {
                "osteoporosis", 
                "osteoporoses"
            }, 
            {
                "parenthesis", 
                "parentheses"
            }, 
            {
                "anastomosis", 
                "anastomoses"
            }, 
            {
                "peristalsis", 
                "peristalses"
            }, 
            {
                "hypothesis", 
                "hypotheses"
            }, 
            {
                "antithesis", 
                "antitheses"
            }, 
            {
                "apotheosis", 
                "apotheoses"
            }, 
            {
                "thrombosis", 
                "thromboses"
            }, 
            {
                "diagnosis", 
                "diagnoses"
            }, 
            {
                "synthesis", 
                "syntheses"
            }, 
            {
                "paralysis", 
                "paralyses"
            }, 
            {
                "prognosis", 
                "prognoses"
            }, 
            {
                "cirrhosis", 
                "cirrhoses"
            }, 
            {
                "sclerosis", 
                "scleroses"
            }, 
            {
                "psychosis", 
                "psychoses"
            }, 
            {
                "apoptosis", 
                "apoptoses"
            }, 
            {
                "symbiosis", 
                "symbioses"
            }
        };

        private static Dictionary<string, string> _wordsEndingWithSusDictionary = new Dictionary<string, string>
        {
            {
                "consensus", 
                "consensuses"
            }, 
            {
                "census", 
                "censuses"
            }
        };

        private static Dictionary<string, string> _wordsEndingWithInxAnxYnxDictionary = new Dictionary<string, string>
        {
            {
                "sphinx", 
                "sphinxes"
            }, 
            {
                "larynx", 
                "larynges"
            }, 
            {
                "lynx", 
                "lynxes"
            }, 
            {
                "pharynx", 
                "pharynxes"
            }, 
            {
                "phalanx", 
                "phalanxes"
            }
        };

        static EnglishPluralizationHelper()
        {
            _userDictionary = new BidirectionalDictionary<string, string>();
            _irregularPluralsPluralizationService =
                new StringBidirectionalDictionary(_irregularPluralsDictionary);
            _assimilatedClassicalInflectionPluralizationService =
                new StringBidirectionalDictionary(_assimilatedClassicalInflectionDictionary);
            _oSuffixPluralizationService = new StringBidirectionalDictionary(_oSuffixDictionary);
            _classicalInflectionPluralizationService =
                new StringBidirectionalDictionary(_classicalInflectionDictionary);
            _wordsEndingWithSePluralizationService =
                new StringBidirectionalDictionary(_wordsEndingWithSeDictionary);
            _wordsEndingWithSisPluralizationService =
                new StringBidirectionalDictionary(_wordsEndingWithSisDictionary);
            _wordsEndingWithSusPluralizationService =
                new StringBidirectionalDictionary(_wordsEndingWithSusDictionary);
            _wordsEndingWithInxAnxYnxPluralizationService =
                new StringBidirectionalDictionary(_wordsEndingWithInxAnxYnxDictionary);
            _irregularVerbPluralizationService = new StringBidirectionalDictionary(_irregularVerbList);
            _knownSingluarWords =
                new List<string>(
                    _irregularPluralsDictionary.Keys.Concat(_assimilatedClassicalInflectionDictionary.Keys)
                        .Concat(_oSuffixDictionary.Keys)
                        .Concat(_classicalInflectionDictionary.Keys)
                        .Concat(_irregularVerbList.Keys)
                        .Concat(_irregularPluralsDictionary.Keys)
                        .Concat(_wordsEndingWithSeDictionary.Keys)
                        .Concat(_wordsEndingWithSisDictionary.Keys)
                        .Concat(_wordsEndingWithSusDictionary.Keys)
                        .Concat(_wordsEndingWithInxAnxYnxDictionary.Keys)
                        .Concat(_uninflectiveWordList)
                        .Except(_knownConflictingPluralList));
            _knownPluralWords =
                new List<string>(
                    _irregularPluralsDictionary.Values.Concat(_assimilatedClassicalInflectionDictionary.Values)
                        .Concat(_oSuffixDictionary.Values)
                        .Concat(_classicalInflectionDictionary.Values)
                        .Concat(_irregularVerbList.Values)
                        .Concat(_irregularPluralsDictionary.Values)
                        .Concat(_wordsEndingWithSeDictionary.Values)
                        .Concat(_wordsEndingWithSisDictionary.Values)
                        .Concat(_wordsEndingWithSusDictionary.Values)
                        .Concat(_wordsEndingWithInxAnxYnxDictionary.Values)
                        .Concat(_uninflectiveWordList));
        }

        public static void AddWord(string singular, string plural)
        {
            Guard.ArgumentNotNull(singular, "singular");
            Guard.ArgumentNotNull(plural, "plural");
            if (_userDictionary.ExistsInSecond(plural))
            {
                throw new ArgumentException("DuplicateEntryInUserDictionary - plural - " + plural);
            }

            if (_userDictionary.ExistsInFirst(singular))
            {
                throw new ArgumentException("DuplicateEntryInUserDictionary - singular - " + singular);
            }

            _userDictionary.AddValue(singular, plural);
        }

        private static string Capitalize(string word, Func<string, string> action)
        {
            string text = action(word);
            if (!IsCapitalized(word))
            {
                return text;
            }

            if (text.Length == 0)
            {
                return text;
            }

            StringBuilder stringBuilder = new StringBuilder(text.Length);
            stringBuilder.Append(char.ToUpperInvariant(text[0]));
            stringBuilder.Append(text.Substring(1));
            return stringBuilder.ToString();
        }

        private static string GetSuffixWord(string word, out string prefixWord)
        {
            int num = word.LastIndexOf(' ');
            prefixWord = word.Substring(0, num + 1);
            return word.Substring(num + 1);
        }

        private static string InternalPluralize(string word)
        {
            if (_userDictionary.ExistsInFirst(word))
            {
                return _userDictionary.GetSecondValue(word);
            }

            if (IsNoOpWord(word))
            {
                return word;
            }

            string str;
            string suffixWord = GetSuffixWord(word, out str);
            if (IsNoOpWord(suffixWord))
            {
                return str + suffixWord;
            }

            if (IsUninflective(suffixWord))
            {
                return str + suffixWord;
            }

            if (_knownPluralWords.Contains(suffixWord.ToLowerInvariant()) || IsPlural(suffixWord))
            {
                return str + suffixWord;
            }

            if (_irregularPluralsPluralizationService.ExistsInFirst(suffixWord))
            {
                return str + _irregularPluralsPluralizationService.GetSecondValue(suffixWord);
            }

            string str2;
            if (TryInflectOnSuffixInWord(suffixWord, new List<string>
            {
                "man"
            }, (string s) => s.Remove(s.Length - 2, 2) + "en", _culture, out str2))
            {
                return str + str2;
            }

            if (TryInflectOnSuffixInWord(suffixWord, new List<string>
            {
                "louse", 
                "mouse"
            }, (string s) => s.Remove(s.Length - 4, 4) + "ice", _culture, out str2))
            {
                return str + str2;
            }

            if (TryInflectOnSuffixInWord(suffixWord, new List<string>
            {
                "tooth"
            }, (string s) => s.Remove(s.Length - 4, 4) + "eeth", _culture, out str2))
            {
                return str + str2;
            }

            if (TryInflectOnSuffixInWord(suffixWord, new List<string>
            {
                "goose"
            }, (string s) => s.Remove(s.Length - 4, 4) + "eese", _culture, out str2))
            {
                return str + str2;
            }

            if (TryInflectOnSuffixInWord(suffixWord, new List<string>
            {
                "foot"
            }, (string s) => s.Remove(s.Length - 3, 3) + "eet", _culture, out str2))
            {
                return str + str2;
            }

            if (TryInflectOnSuffixInWord(suffixWord, new List<string>
            {
                "zoon"
            }, (string s) => s.Remove(s.Length - 3, 3) + "oa", _culture, out str2))
            {
                return str + str2;
            }

            if (TryInflectOnSuffixInWord(suffixWord, new List<string>
            {
                "cis", 
                "sis", 
                "xis"
            }, (string s) => s.Remove(s.Length - 2, 2) + "es", _culture, out str2))
            {
                return str + str2;
            }

            if (_assimilatedClassicalInflectionPluralizationService.ExistsInFirst(suffixWord))
            {
                return str + _assimilatedClassicalInflectionPluralizationService.GetSecondValue(suffixWord);
            }

            if (_classicalInflectionPluralizationService.ExistsInFirst(suffixWord))
            {
                return str + _classicalInflectionPluralizationService.GetSecondValue(suffixWord);
            }

            if (TryInflectOnSuffixInWord(suffixWord, new List<string>
            {
                "trix"
            }, (string s) => s.Remove(s.Length - 1, 1) + "ces", _culture, out str2))
            {
                return str + str2;
            }

            if (TryInflectOnSuffixInWord(suffixWord, new List<string>
            {
                "eau", 
                "ieu"
            }, (string s) => s + "x", _culture, out str2))
            {
                return str + str2;
            }

            if (_wordsEndingWithInxAnxYnxPluralizationService.ExistsInFirst(suffixWord))
            {
                return str + _wordsEndingWithInxAnxYnxPluralizationService.GetSecondValue(suffixWord);
            }

            if (TryInflectOnSuffixInWord(suffixWord, new List<string>
            {
                "ch", 
                "sh", 
                "ss"
            }, (string s) => s + "es", _culture, out str2))
            {
                return str + str2;
            }

            if (TryInflectOnSuffixInWord(suffixWord, new List<string>
            {
                "alf", 
                "elf", 
                "olf", 
                "eaf", 
                "arf"
            }, delegate(string s)
            {
                if (!s.EndsWith("deaf", true, _culture))
                {
                    return s.Remove(s.Length - 1, 1) + "ves";
                }

                return s;
            }, _culture, out str2))
            {
                return str + str2;
            }

            if (TryInflectOnSuffixInWord(suffixWord, new List<string>
            {
                "nife", 
                "life", 
                "wife"
            }, (string s) => s.Remove(s.Length - 2, 2) + "ves", _culture, out str2))
            {
                return str + str2;
            }

            if (TryInflectOnSuffixInWord(suffixWord, new List<string>
            {
                "ay", 
                "ey", 
                "iy", 
                "oy", 
                "uy"
            }, (string s) => s + "s", _culture, out str2))
            {
                return str + str2;
            }

            if (suffixWord.EndsWith("y", true, _culture))
            {
                return str + suffixWord.Remove(suffixWord.Length - 1, 1) + "ies";
            }

            if (_oSuffixPluralizationService.ExistsInFirst(suffixWord))
            {
                return str + _oSuffixPluralizationService.GetSecondValue(suffixWord);
            }

            if (TryInflectOnSuffixInWord(suffixWord, new List<string>
            {
                "ao", 
                "eo", 
                "io", 
                "oo", 
                "uo"
            }, (string s) => s + "s", _culture, out str2))
            {
                return str + str2;
            }

            if (suffixWord.EndsWith("o", true, _culture) || suffixWord.EndsWith("s", true, _culture))
            {
                return str + suffixWord + "es";
            }

            if (suffixWord.EndsWith("x", true, _culture))
            {
                return str + suffixWord + "es";
            }

            return str + suffixWord + "s";
        }

        private static string InternalSingularize(string word)
        {
            if (_userDictionary.ExistsInSecond(word))
            {
                return _userDictionary.GetFirstValue(word);
            }

            if (IsNoOpWord(word))
            {
                return word;
            }

            string str;
            string suffixWord = GetSuffixWord(word, out str);
            if (IsNoOpWord(suffixWord))
            {
                return str + suffixWord;
            }

            if (IsUninflective(suffixWord))
            {
                return str + suffixWord;
            }

            if (_knownSingluarWords.Contains(suffixWord.ToLowerInvariant()))
            {
                return str + suffixWord;
            }

            if (_irregularVerbPluralizationService.ExistsInSecond(suffixWord))
            {
                return str + _irregularVerbPluralizationService.GetFirstValue(suffixWord);
            }

            if (_irregularPluralsPluralizationService.ExistsInSecond(suffixWord))
            {
                return str + _irregularPluralsPluralizationService.GetFirstValue(suffixWord);
            }

            if (_wordsEndingWithSisPluralizationService.ExistsInSecond(suffixWord))
            {
                return str + _wordsEndingWithSisPluralizationService.GetFirstValue(suffixWord);
            }

            if (_wordsEndingWithSePluralizationService.ExistsInSecond(suffixWord))
            {
                return str + _wordsEndingWithSePluralizationService.GetFirstValue(suffixWord);
            }

            if (_wordsEndingWithSusPluralizationService.ExistsInSecond(suffixWord))
            {
                return str + _wordsEndingWithSusPluralizationService.GetFirstValue(suffixWord);
            }

            string str2;
            if (TryInflectOnSuffixInWord(suffixWord, new List<string>
            {
                "men"
            }, (string s) => s.Remove(s.Length - 2, 2) + "an", _culture, out str2))
            {
                return str + str2;
            }

            if (TryInflectOnSuffixInWord(suffixWord, new List<string>
            {
                "lice", 
                "mice"
            }, (string s) => s.Remove(s.Length - 3, 3) + "ouse", _culture, out str2))
            {
                return str + str2;
            }

            if (TryInflectOnSuffixInWord(suffixWord, new List<string>
            {
                "teeth"
            }, (string s) => s.Remove(s.Length - 4, 4) + "ooth", _culture, out str2))
            {
                return str + str2;
            }

            if (TryInflectOnSuffixInWord(suffixWord, new List<string>
            {
                "geese"
            }, (string s) => s.Remove(s.Length - 4, 4) + "oose", _culture, out str2))
            {
                return str + str2;
            }

            if (TryInflectOnSuffixInWord(suffixWord, new List<string>
            {
                "feet"
            }, (string s) => s.Remove(s.Length - 3, 3) + "oot", _culture, out str2))
            {
                return str + str2;
            }

            if (TryInflectOnSuffixInWord(suffixWord, new List<string>
            {
                "zoa"
            }, (string s) => s.Remove(s.Length - 2, 2) + "oon", _culture, out str2))
            {
                return str + str2;
            }

            if (TryInflectOnSuffixInWord(suffixWord, new List<string>
            {
                "ches", 
                "shes", 
                "sses"
            }, (string s) => s.Remove(s.Length - 2, 2), _culture, out str2))
            {
                return str + str2;
            }

            if (_assimilatedClassicalInflectionPluralizationService.ExistsInSecond(suffixWord))
            {
                return str + _assimilatedClassicalInflectionPluralizationService.GetFirstValue(suffixWord);
            }

            if (_classicalInflectionPluralizationService.ExistsInSecond(suffixWord))
            {
                return str + _classicalInflectionPluralizationService.GetFirstValue(suffixWord);
            }

            if (TryInflectOnSuffixInWord(suffixWord, new List<string>
            {
                "trices"
            }, (string s) => s.Remove(s.Length - 3, 3) + "x", _culture, out str2))
            {
                return str + str2;
            }

            if (TryInflectOnSuffixInWord(suffixWord, new List<string>
            {
                "eaux", 
                "ieux"
            }, (string s) => s.Remove(s.Length - 1, 1), _culture, out str2))
            {
                return str + str2;
            }

            if (_wordsEndingWithInxAnxYnxPluralizationService.ExistsInSecond(suffixWord))
            {
                return str + _wordsEndingWithInxAnxYnxPluralizationService.GetFirstValue(suffixWord);
            }

            if (TryInflectOnSuffixInWord(suffixWord, new List<string>
            {
                "alves", 
                "elves", 
                "olves", 
                "eaves", 
                "arves"
            }, (string s) => s.Remove(s.Length - 3, 3) + "f", _culture, out str2))
            {
                return str + str2;
            }

            if (TryInflectOnSuffixInWord(suffixWord, new List<string>
            {
                "nives", 
                "lives", 
                "wives"
            }, (string s) => s.Remove(s.Length - 3, 3) + "fe", _culture, out str2))
            {
                return str + str2;
            }

            if (TryInflectOnSuffixInWord(suffixWord, new List<string>
            {
                "ays", 
                "eys", 
                "iys", 
                "oys", 
                "uys"
            }, (string s) => s.Remove(s.Length - 1, 1), _culture, out str2))
            {
                return str + str2;
            }

            if (suffixWord.EndsWith("ies", true, _culture))
            {
                return str + suffixWord.Remove(suffixWord.Length - 3, 3) + "y";
            }

            if (_oSuffixPluralizationService.ExistsInSecond(suffixWord))
            {
                return str + _oSuffixPluralizationService.GetFirstValue(suffixWord);
            }

            if (TryInflectOnSuffixInWord(suffixWord, new List<string>
            {
                "aos", 
                "eos", 
                "ios", 
                "oos", 
                "uos"
            }, (string s) => suffixWord.Remove(suffixWord.Length - 1, 1), _culture, out str2))
            {
                return str + str2;
            }

            if (TryInflectOnSuffixInWord(suffixWord, new List<string>
            {
                "ces"
            }, (string s) => s.Remove(s.Length - 1, 1), _culture, out str2))
            {
                return str + str2;
            }

            if (TryInflectOnSuffixInWord(suffixWord, new List<string>
            {
                "ces", 
                "ses", 
                "xes"
            }, (string s) => s.Remove(s.Length - 2, 2), _culture, out str2))
            {
                return str + str2;
            }

            if (suffixWord.EndsWith("oes", true, _culture))
            {
                return str + suffixWord.Remove(suffixWord.Length - 2, 2);
            }

            if (suffixWord.EndsWith("ss", true, _culture))
            {
                return str + suffixWord;
            }

            if (suffixWord.EndsWith("s", true, _culture))
            {
                return str + suffixWord.Remove(suffixWord.Length - 1, 1);
            }

            return str + suffixWord;
        }

        private static bool IsAlphabets(string word)
        {
            return !string.IsNullOrEmpty(word.Trim()) && word.Equals(word.Trim()) &&
                   !Regex.IsMatch(word, "[^a-zA-Z\\s]");
        }

        private static bool IsCapitalized(string word)
        {
            return !string.IsNullOrEmpty(word) && char.IsUpper(word, 0);
        }

        private static bool IsNoOpWord(string word)
        {
            return !IsAlphabets(word) || word.Length <= 1 || _pronounList.Contains(word.ToLowerInvariant());
        }

        public static bool IsPlural(string word)
        {
            Guard.ArgumentNotNull(word, "word");
            return _userDictionary.ExistsInSecond(word) ||
                   (!_userDictionary.ExistsInFirst(word) &&
                    (IsUninflective(word) || _knownPluralWords.Contains(word.ToLower(_culture)) ||
                     !Singularize(word).Equals(word)));
        }

        public static bool IsSingular(string word)
        {
            Guard.ArgumentNotNull(word, "word");
            return _userDictionary.ExistsInFirst(word) ||
                   (!_userDictionary.ExistsInSecond(word) &&
                    (IsUninflective(word) || _knownSingluarWords.Contains(word.ToLower(_culture)) ||
                     (!IsNoOpWord(word) && Singularize(word).Equals(word))));
        }

        private static bool IsUninflective(string word)
        {
            Guard.ArgumentNotNull(word, "word");
            return DoesWordContainSuffix(word, _uninflectiveSuffixList, _culture) ||
                   (!word.ToLower(_culture).Equals(word) && word.EndsWith("ese", false, _culture)) ||
                   _uninflectiveWordList.Contains(word.ToLowerInvariant());
        }

        public static string Pluralize(string word)
        {
            Guard.ArgumentNotNull(word, "word");
            return Capitalize(word, new Func<string, string>(InternalPluralize));
        }

        public static string Singularize(string word)
        {
            Guard.ArgumentNotNull(word, "word");
            return Capitalize(word, new Func<string, string>(InternalSingularize));
        }

        internal static bool DoesWordContainSuffix(string word, IEnumerable<string> suffixes, CultureInfo culture)
        {
            return suffixes.Any((string s) => word.EndsWith(s, true, culture));
        }

        internal static bool TryInflectOnSuffixInWord(string word, IEnumerable<string> suffixes, 
            Func<string, string> operationOnWord, CultureInfo culture, out string newWord)
        {
            newWord = null;
            if (DoesWordContainSuffix(word, suffixes, culture))
            {
                newWord = operationOnWord(word);
                return true;
            }

            return false;
        }
    }
}
