namespace FixMath.NET {
	public partial struct Fix64 {
		const int LARGE_PI_RAW = 1686629713;
		const int LARGE_PI_TIMES = 15;

        internal static readonly int[] SinLut = new int[402] {
			0x0, 0x40, 0x80, 0xC0, 0x100, 0x140, 0x181, 0x1C1, 
			0x201, 0x241, 0x281, 0x2C1, 0x301, 0x341, 0x382, 0x3C2, 
			0x402, 0x442, 0x482, 0x4C2, 0x502, 0x542, 0x582, 0x5C2, 
			0x602, 0x641, 0x681, 0x6C1, 0x701, 0x741, 0x780, 0x7C0, 
			0x800, 0x840, 0x87F, 0x8BF, 0x8FE, 0x93E, 0x97D, 0x9BD, 
			0x9FC, 0xA3C, 0xA7B, 0xABA, 0xAF9, 0xB39, 0xB78, 0xBB7, 
			0xBF6, 0xC35, 0xC74, 0xCB3, 0xCF2, 0xD31, 0xD6F, 0xDAE, 
			0xDED, 0xE2B, 0xE6A, 0xEA8, 0xEE7, 0xF25, 0xF64, 0xFA2, 
			0xFE0, 0x101E, 0x105C, 0x109A, 0x10D8, 0x1116, 0x1154, 0x1192, 
			0x11CF, 0x120D, 0x124B, 0x1288, 0x12C5, 0x1303, 0x1340, 0x137D, 
			0x13BA, 0x13F7, 0x1434, 0x1471, 0x14AE, 0x14EB, 0x1527, 0x1564, 
			0x15A0, 0x15DC, 0x1619, 0x1655, 0x1691, 0x16CD, 0x1709, 0x1745, 
			0x1781, 0x17BC, 0x17F8, 0x1833, 0x186F, 0x18AA, 0x18E5, 0x1920, 
			0x195B, 0x1996, 0x19D1, 0x1A0B, 0x1A46, 0x1A80, 0x1ABB, 0x1AF5, 
			0x1B2F, 0x1B69, 0x1BA3, 0x1BDD, 0x1C17, 0x1C50, 0x1C8A, 0x1CC3, 
			0x1CFD, 0x1D36, 0x1D6F, 0x1DA8, 0x1DE0, 0x1E19, 0x1E52, 0x1E8A, 
			0x1EC3, 0x1EFB, 0x1F33, 0x1F6B, 0x1FA3, 0x1FDA, 0x2012, 0x2049, 
			0x2081, 0x20B8, 0x20EF, 0x2126, 0x215D, 0x2194, 0x21CA, 0x2201, 
			0x2237, 0x226D, 0x22A3, 0x22D9, 0x230F, 0x2344, 0x237A, 0x23AF, 
			0x23E4, 0x241A, 0x244E, 0x2483, 0x24B8, 0x24EC, 0x2521, 0x2555, 
			0x2589, 0x25BD, 0x25F1, 0x2624, 0x2658, 0x268B, 0x26BE, 0x26F1, 
			0x2724, 0x2757, 0x2789, 0x27BC, 0x27EE, 0x2820, 0x2852, 0x2884, 
			0x28B5, 0x28E7, 0x2918, 0x2949, 0x297A, 0x29AB, 0x29DC, 0x2A0C, 
			0x2A3C, 0x2A6C, 0x2A9C, 0x2ACC, 0x2AFC, 0x2B2B, 0x2B5B, 0x2B8A, 
			0x2BB9, 0x2BE7, 0x2C16, 0x2C44, 0x2C73, 0x2CA1, 0x2CCF, 0x2CFC, 
			0x2D2A, 0x2D57, 0x2D85, 0x2DB2, 0x2DDE, 0x2E0B, 0x2E38, 0x2E64, 
			0x2E90, 0x2EBC, 0x2EE8, 0x2F13, 0x2F3F, 0x2F6A, 0x2F95, 0x2FC0, 
			0x2FEA, 0x3015, 0x303F, 0x3069, 0x3093, 0x30BD, 0x30E6, 0x3110, 
			0x3139, 0x3162, 0x318A, 0x31B3, 0x31DB, 0x3203, 0x322B, 0x3253, 
			0x327B, 0x32A2, 0x32C9, 0x32F0, 0x3317, 0x333D, 0x3364, 0x338A, 
			0x33B0, 0x33D6, 0x33FB, 0x3420, 0x3446, 0x346A, 0x348F, 0x34B4, 
			0x34D8, 0x34FC, 0x3520, 0x3544, 0x3567, 0x358A, 0x35AD, 0x35D0, 
			0x35F3, 0x3615, 0x3638, 0x365A, 0x367B, 0x369D, 0x36BE, 0x36DF, 
			0x3700, 0x3721, 0x3741, 0x3762, 0x3782, 0x37A2, 0x37C1, 0x37E1, 
			0x3800, 0x381F, 0x383E, 0x385C, 0x387A, 0x3898, 0x38B6, 0x38D4, 
			0x38F1, 0x390E, 0x392B, 0x3948, 0x3965, 0x3981, 0x399D, 0x39B9, 
			0x39D4, 0x39F0, 0x3A0B, 0x3A26, 0x3A41, 0x3A5B, 0x3A75, 0x3A8F, 
			0x3AA9, 0x3AC3, 0x3ADC, 0x3AF5, 0x3B0E, 0x3B26, 0x3B3F, 0x3B57, 
			0x3B6F, 0x3B87, 0x3B9E, 0x3BB5, 0x3BCC, 0x3BE3, 0x3BFA, 0x3C10, 
			0x3C26, 0x3C3C, 0x3C51, 0x3C67, 0x3C7C, 0x3C91, 0x3CA5, 0x3CBA, 
			0x3CCE, 0x3CE2, 0x3CF5, 0x3D09, 0x3D1C, 0x3D2F, 0x3D41, 0x3D54, 
			0x3D66, 0x3D78, 0x3D8A, 0x3D9B, 0x3DAD, 0x3DBE, 0x3DCE, 0x3DDF, 
			0x3DEF, 0x3DFF, 0x3E0F, 0x3E1F, 0x3E2E, 0x3E3D, 0x3E4C, 0x3E5A, 
			0x3E69, 0x3E77, 0x3E85, 0x3E92, 0x3EA0, 0x3EAD, 0x3EBA, 0x3EC6, 
			0x3ED3, 0x3EDF, 0x3EEB, 0x3EF6, 0x3F02, 0x3F0D, 0x3F18, 0x3F22, 
			0x3F2D, 0x3F37, 0x3F41, 0x3F4A, 0x3F54, 0x3F5D, 0x3F66, 0x3F6E, 
			0x3F77, 0x3F7F, 0x3F87, 0x3F8E, 0x3F96, 0x3F9D, 0x3FA4, 0x3FAB, 
			0x3FB1, 0x3FB7, 0x3FBD, 0x3FC3, 0x3FC8, 0x3FCD, 0x3FD2, 0x3FD7, 
			0x3FDB, 0x3FDF, 0x3FE3, 0x3FE7, 0x3FEA, 0x3FED, 0x3FF0, 0x3FF3, 
			0x3FF5, 0x3FF7, 0x3FF9, 0x3FFB, 0x3FFC, 0x3FFD, 0x3FFE, 0x3FFF, 
			0x3FFF, 0x4000, 
		};
	}
}