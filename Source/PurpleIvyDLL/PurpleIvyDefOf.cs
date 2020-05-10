using System;
using RimWorld;
using Verse;

namespace PurpleIvy
{
	[DefOf]
	public static class PurpleIvyDefOf
	{
		public static HediffDef PoisonousPurpleHediff;

        public static HediffDef MucilageslimeHediff;

        public static HediffDef HarmfulBacteriaHediff;

        public static HediffDef PI_CrashlandedDowned;

        public static HediffDef PI_Regen;

        public static HediffDef PI_AlienBlood;

        public static HediffDef PI_VaporToxicFilth;

        public static HediffDef PI_AlienInfection;

        public static HediffDef PI_AlienMutation;

        public static HediffDef PI_GainConsciousness;

        public static DamageDef AlienToxicSting;

        public static DamageDef PI_ToxicExplosion;

        public static FactionDef Genny;

        public static FactionDef KorsolianFaction;

        public static JobDef PI_Kill;

        public static JobDef PI_ThrowSmoke;

        public static JobDef PI_AttackMelee;

        public static JobDef PI_JumpOnTarget;

        public static JobDef PI_ExtractNectar;

        public static JobDef PI_DrawAlienBlood;

        public static JobDef PI_DrawKorsolianToxin;

        public static JobDef PI_TakeAlienToContainmentBreach;

        public static JobDef PI_ConductResearchOnAliens;

        public static JobDef PI_BiomaterialsStudy;

        public static JobDef PI_PreciseVivisection;

        public static JobDef PI_AnimalRangeAttack;

        public static JobDef PI_HaulToCell;

        public static JobDef PI_EntangleTargetWithSlugs;

        public static RecipeDef DrawAlienBlood;

        public static RecipeDef DrawAlphaAlienBlood;

        public static RecipeDef DrawBetaAlienBlood;

        public static RecipeDef DrawGammaAlienBlood;

        public static RecipeDef DrawOmegaAlienBlood;

        public static RecipeDef DrawGuardAlienBlood;

        public static RecipeDef DrawKorsolianToxin;

        public static RecipeDef PreciseVivisectionAlpha;

        public static RecipeDef PreciseVivisectionBeta;

        public static RecipeDef PreciseVivisectionGamma;

        public static RecipeDef PreciseVivisectionOmega;

        public static RecipeDef PreciseVivisectionGuard;

        public static RecipeDef PreciseVivisectionQueen;

        public static RecipeDef PI_AlienStudyRecipe;

        public static RecipeDef PI_BiomaterialsStudyRecipe;

        public static ThingDef InfectedCorpseDummy;

        public static ThingDef PurpleIvy;

        public static ThingDef PI_KorsolianToxin;

        public static ThingDef AlienBloodFilth;

        public static ThingDef PI_ToxicFilth;

        public static ThingDef PI_PowerBeam;

        public static ThingDef PI_ExplosionPlus;

        public static ThingDef PI_Spores;

        public static WeatherDef PI_PurpleFog;

        public static WeatherDef PI_PurpleFoggyRain;

        public static WeatherDef PI_EMPStorm;

        public static GameConditionDef PurpleFogGameCondition;

        public static ThingDef Genny_ParasiteAlpha;

        public static ThingDef Genny_ParasiteBeta;

        public static ThingDef Genny_ParasiteGamma;

        public static ThingDef Genny_ParasiteOmega;

        public static ThingDef Genny_ParasiteNestGuard;

        public static ThingDef PI_Nest;

        public static ThingDef EggSac;

        public static ThingDef EggSacBeta;

        public static ThingDef EggSacGamma;

        public static ThingDef EggSacNestGuard;

        public static ThingDef ParasiteEgg;

        public static ThingDef GenTurretBase;

        public static ThingDef Turret_GenMortarSeed;

        public static ThingDef GasPump;

        public static ThingDef PlantVenomousToothwort;

        public static ThingDef PI_Nectar;

        public static ThingDef PI_ContainmentBreach;

        public static ThingDef PI_AlphaBlood;

        public static ThingDef PI_BetaBlood;

        public static ThingDef PI_GammaBlood;

        public static ThingDef PI_OmegaBlood;

        public static WorldObjectDef PI_InfectedTile;

        public static WorldObjectDef PI_AbandonedBase;

        public static WorldObjectDef PI_DefeatedBase;

        public static SitePartDef InfectedSite;

        public static IncidentDef PI_AlienRaid;

        public static ThingDef EMP_Sparks;

        public static ThingDef EMPGlow;

        public static ThingDef Mote_EMPSmoke;

        public static ThingDef PI_CorruptedTree;

        public static DamageDef PI_ToxicBurn;

        public static ThingCategoryDef CorpsesAlienParasiteAlpha;

        public static ThingCategoryDef CorpsesAlienParasiteBeta;

        public static ThingCategoryDef CorpsesAlienParasiteGamma;

        public static ThingCategoryDef CorpsesAlienParasiteOmega;

        public static ThingCategoryDef CorpsesAlienParasiteGuard;

        public static ThingCategoryDef CorpsesAlienParasiteQueen;

        public static PawnKindDef Genny_Queen;

        public static PawnKindDef KorsolianSoldier;

        public static ThinkTreeDef PI_HumanlikeMutant;

        public static SitePartDef PI_CrashedShip;

        public static ResearchProjectDef PI_BasicBioprotection;
        public static ResearchProjectDef PI_AdvBioprotection;
        public static ResearchProjectDef PI_Biolab;
        public static ResearchProjectDef PI_AlienContainment;
        public static ResearchProjectDef PI_AdvAlienContainment;
        public static ResearchProjectDef PI_Vivisection;
        public static ResearchProjectDef PI_ResourceExtraction;
        public static ResearchProjectDef PI_BasicDrugs;
        public static ResearchProjectDef PI_ResourceProcession;
        public static ResearchProjectDef PI_AdvDrugs;
        public static ResearchProjectDef PI_MutagensA;
        public static ResearchProjectDef PI_MutagensB;
        public static ResearchProjectDef PI_MutagensC;
        public static ResearchProjectDef PI_BasicBionics;
        public static ResearchProjectDef PI_AdvBionics;
        public static ResearchProjectDef PI_Implantation;
        public static ResearchProjectDef PI_BioRepl;
        public static ResearchProjectDef PI_LaserBench;
        public static ResearchProjectDef PI_AlienArmorA;
        public static ResearchProjectDef PI_AlienArmorB;
        public static ResearchProjectDef PI_AlienArmorC;
        public static ResearchProjectDef PI_AlienWeaponsA;
        public static ResearchProjectDef PI_AlienWeaponsB;
        public static ResearchProjectDef PI_AlienWeaponsC;

        public static ThingDef KorsolianLeather;
        public static ThingDef PI_AlienLeather;
        public static ThingDef PI_AlphaNervousSystem;
        public static ThingDef PI_BetaNervousSystem;
        public static ThingDef PI_BigTentacles;
        public static ThingDef PI_GammaNervousSystem;
        public static ThingDef PI_NestGuardNervousSystem;
        public static ThingDef PI_NeuralLiquid;
        public static ThingDef PI_OmegaNervousSystem;
        public static ThingDef PI_PlasmaContainer;
        public static ThingDef PI_PlasmaNucleus;
        public static ThingDef PI_QueenNervousSystem;
        public static ThingDef PI_Tentacles;
        public static ThingDef PI_ToxicSac;
        public static ThingDef PI_StickySlugs;

    }
}

