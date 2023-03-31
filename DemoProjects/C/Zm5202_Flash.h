
#ifndef ZM5202_FLASH_H_
#define ZM5202_FLASH_H_


#include "main.h"
#include "SerialApi_Boot_US.h"
#include "SerialApi_Boot_EU.h"
#include "stdbool.h"
#include "spi.h"
#include "usart.h"
#include "SerialApi_NVR.h"

#define t 0
//#define ZW_Uart huart2
#define ZW_Spi hspi2

typedef union Num16_
{
	uint8_t ar[2];
	uint16_t n;
}Num16;

typedef struct ZM5202_CMD_Struct_
{
	uint8_t b0;
	uint8_t b1;
	uint8_t b2;
	uint8_t b3;
}ZM5202_CMD_Struct;

typedef struct ZM5202_Write_Struct_
{
	uint8_t b0;
	Num16 b1;
	uint8_t b2;

} ZM5202_Write_Struct;

typedef union ZM_5205_CMD_
{
	ZM5202_CMD_Struct GneralCmd;
	ZM5202_Write_Struct WriteCmd;


} ZM_5205_CMD;

typedef enum TX_PowerSetting_
{
	High= 0x00,
	Medium= 0x01,//Always part of SN response message
	Low =0x02,

}TX_PowerSetting;


void TxZwave();
void Test();
bool BusyProgramming();

void Zm5202Flash_Lib_Init();

void HAL_UART_RxCpltCallback(UART_HandleTypeDef *);
void HAL_SPI_TxRxCpltCallback(SPI_HandleTypeDef*);

void SpiSetup();
void ReadSignature();
void CheckState();
void Reset();

void WriteSRAM(uint16_t ,uint8_t );
void ReadSRAM(uint16_t);
void WriteFlash(uint8_t);
void ReadFlash(uint8_t);
void ContinueWrite(uint8_t,uint8_t,uint8_t);
void ContinueRead();

void EraseChip();
void EraseSector(uint8_t);

void DisableEoosMode();
void ClearAutoProg1();

void WriteLockBits(uint8_t , uint8_t);
void ReadLockBits(uint8_t);
void ReadAllLockBits();
void WriteNVR(uint8_t, uint8_t);
void ReadNVR(uint8_t );
void WriteNVRAll();
void ReadNVRAll();

void UpdateTxPowerSettings();
void WriteCRC();
void RunCRC();//takes 22ms;

void WriteZwaveCodeToFlash();
void WriteZwaveCodeToFlash1();
void ReadAllFlash();


void ProgramZM5202Module();

#endif /* ZM5202_FLASH_H_ */
